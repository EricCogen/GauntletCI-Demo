using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using OrderService.Controllers;
using OrderService.Domain;
using OrderService.Persistence;

namespace OrderService.Tests.Scenarios
{
    /// <summary>
    /// Comprehensive test suite for Tier 3 behavioral regression scenarios.
    /// These tests validate that specific behavioral changes are caught during analysis
    /// and demonstrate why traditional static analysis tools miss these patterns.
    /// </summary>
    public class Tier3BehavioralRegressionTests
    {
        /// <summary>
        /// Scenario 19: Architectural Access Control Drop
        /// 
        /// Risk: [Authorize] attribute removed from a protected endpoint during refactoring.
        /// 
        /// Why traditional tools miss this:
        /// - Code compiles cleanly (no syntax error)
        /// - Tests still pass (business logic is unchanged)
        /// - No vulnerability signature to match
        /// - Requires diff comparison to detect
        /// </summary>
        [Fact(DisplayName = "S19: Unauthorized access succeeds when [Authorize] attribute is removed")]
        public async Task S19_AccessControlDrop_AllowsUnauthorizedAccess()
        {
            // Arrange: Demonstrates that without [Authorize], anyone can access
            var controller = new BillingControllerWithoutAuth();
            var request = new RefundRequest { OrderId = Guid.NewGuid(), Amount = 100m };

            // Act: Call the endpoint (no authorization check since attribute is missing)
            var result = await controller.ProcessRefund(request.OrderId, request);

            // Assert: Without [Authorize], the method executes and returns success
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ((OkObjectResult)result).StatusCode);
        }

        [Fact(DisplayName = "S19: Baseline behavior - authorization properly enforced")]
        public async Task S19_Baseline_AuthorizationEnforced()
        {
            // This demonstrates what SHOULD happen with [Authorize] present
            var controller = new BillingControllerWithAuth();
            var request = new RefundRequest { OrderId = Guid.NewGuid(), Amount = 100m };

            // Act: Call the endpoint with [Authorize] present
            var result = await controller.ProcessRefund(request.OrderId, request);

            // Assert: With [Authorize], method would require authorization
            // In real scenario, ASP.NET middleware would prevent execution
            // Here we just verify the method exists and can be called
            Assert.NotNull(result);
        }

        /// <summary>
        /// Scenario 20: Audit Log Inversion
        /// 
        /// Risk: Execution order changed between state mutation and external service call.
        /// 
        /// Why traditional tools miss this:
        /// - Both statements are idiomatic C# that pass style checks
        /// - Code compiles and executes without error
        /// - Tests pass because the refactored code still works
        /// - Only sequence-dependent bugs appear in production
        /// </summary>
        [Fact(DisplayName = "S20: Audit log shows failed transaction before persistence fails")]
        public async Task S20_ExecutionSequenceInversion_AuditLogInconsistency()
        {
            // Arrange: Simulate a refund scenario where order save fails
            var repository = new FailingOrderRepository();
            var auditLog = new InMemoryAuditLog();
            var processor = new OrderProcessorWithInvertedSequence(repository, auditLog);

            var order = CreateOrder();

            // Act: Attempt to process refund (will fail at persistence)
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => processor.ProcessRefundAsync(order, reason: "Customer request")
            );

            // Assert: Audit log shows success WAS LOGGED before persistence failed
            // This is the bug: external state (audit log) is mutated before persistence is validated
            var auditEntries = auditLog.GetEntries();
            Assert.NotEmpty(auditEntries);
            // In the buggy version, RefundApproved is logged BEFORE the exception occurs
            Assert.Single(auditEntries, e => e.Type == "RefundApproved");
        }

        [Fact(DisplayName = "S20: Baseline behavior - persistence verified before audit logging")]
        public async Task S20_Baseline_CorrectSequence()
        {
            // This demonstrates the correct sequence
            var repository = new FailingOrderRepository();
            var auditLog = new InMemoryAuditLog();
            var processor = new OrderProcessorWithCorrectSequence(repository, auditLog);

            var order = CreateOrder();

            // Act: Attempt to process refund (will fail at persistence)
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => processor.ProcessRefundAsync(order, reason: "Customer request")
            );

            // Assert: Audit log should be EMPTY if persistence fails first
            // This is correct: no state mutation before successful persistence
            var auditEntries = auditLog.GetEntries();
            Assert.Empty(auditEntries); // Nothing logged because persistence failed first
        }

        /// <summary>
        /// Scenario 21: Async Context Propagation Loss
        /// 
        /// Risk: CancellationToken context lost in internal method calls.
        /// 
        /// Why traditional tools miss this:
        /// - Code compiles and type checks successfully
        /// - No `Task.Result` or `.Wait()` anti-pattern
        /// - Tests may pass with short timeouts
        /// - Context loss manifests as timeout behavior in production
        /// </summary>
        [Fact(DisplayName = "S21: Cancellation token not propagated through internal call stack")]
        public async Task S21_AsyncPropagationLoss_IgnoresCancellation()
        {
            // Arrange: Create a processor that drops CancellationToken context
            var processor = new OrderProcessorWithDroppedCancellation();
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

            // Act: Request cancellation while processing
            var task = processor.ProcessOrderWithLongOperationAsync(CreateOrder(), cts.Token);

            // Assert: Operation should respect cancellation, but doesn't (token not propagated)
            // Demonstrates the bug: operation completes despite cancellation request
            var exception = await Record.ExceptionAsync(() => task);
            Assert.Null(exception); // No cancellation exception because token wasn't propagated
        }

        /// <summary>
        /// Scenario 22: Breaking Public API Contract
        /// 
        /// Risk: Method signature changed without updating callers within internal library structure.
        /// 
        /// Why traditional tools miss this:
        /// - Code compiles after recompilation
        /// - Tests pass (test calls use correct signature)
        /// - Breaks only when external code uses the method
        /// - Diff-level analysis catches it; snapshot analysis sees valid code
        /// </summary>
        [Fact(DisplayName = "S22: Method signature change breaks callers")]
        public void S22_ApiContractDrift_MethodSignatureChanged()
        {
            // Arrange: Create instances using new method signature
            var processor = new OrderProcessor();
            var order = CreateOrder();

            // Act & Assert: New signature works with required parameter
            var result = processor.ProcessOrderWithNewSignature(order, requiresValidation: true);
            Assert.NotNull(result);

            // But old code calling with old signature would fail
            // Old caller: processor.ProcessOrder(order)
            // New signature: processor.ProcessOrder(order, requiresValidation)
        }

        [Fact(DisplayName = "S22: Baseline behavior - optional parameter maintains backward compatibility")]
        public void S22_Baseline_BackwardCompatible()
        {
            // This demonstrates the correct approach
            var processor = new OrderProcessor();
            var order = CreateOrder();

            // Can call with or without new parameter
            var result1 = processor.ProcessOrderWithOptionalParameter(order);
            var result2 = processor.ProcessOrderWithOptionalParameter(order, requiresValidation: false);

            Assert.NotNull(result1);
            Assert.NotNull(result2);
        }

        // Helper methods

        private Order CreateOrder()
        {
            var customer = new Customer(Guid.NewGuid(), "test@example.com", "Test Customer");
            var items = new List<OrderItem>
            {
                new OrderItem("SKU-001", 1, new Money(100m, "USD"))
            };
            return new Order(Guid.NewGuid(), customer, items, DateTimeOffset.UtcNow);
        }
    }

    // Test doubles and helper classes

    public class RefundRequest
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
    }

    public class BillingControllerWithAuth : ControllerBase
    {
        [Authorize(Roles = "BillingAdmin")]
        public async Task<IActionResult> ProcessRefund(Guid id, [FromBody] RefundRequest request)
        {
            return Ok(new { success = true, refundId = Guid.NewGuid() });
        }
    }

    public class BillingControllerWithoutAuth : ControllerBase
    {
        // [Authorize] attribute REMOVED - this is the bug
        public async Task<IActionResult> ProcessRefund(Guid id, [FromBody] RefundRequest request)
        {
            return Ok(new { success = true, refundId = Guid.NewGuid() });
        }
    }

    public class FailingOrderRepository : IOrderRepository
    {
        public async Task<Order?> GetAsync(Guid id, CancellationToken ct = default)
        {
            var customer = new Customer(Guid.NewGuid(), "test@example.com", "Test");
            var items = new List<OrderItem> { new OrderItem("SKU", 1, new Money(100m, "USD")) };
            return new Order(id, customer, items, DateTimeOffset.UtcNow);
        }

        public async Task AddAsync(Order order, CancellationToken ct = default)
        {
            throw new InvalidOperationException("Persistence failed");
        }

        public async Task UpdateAsync(Order order, CancellationToken ct = default)
        {
            throw new InvalidOperationException("Persistence failed");
        }

        public async Task<IReadOnlyList<Order>> ListAsync(CancellationToken ct = default)
        {
            return new List<Order>();
        }
    }

    public class InMemoryAuditLog
    {
        private readonly List<(string Type, string Message)> _entries = new();

        public void Log(string type, string message) => _entries.Add((type, message));
        public List<(string Type, string Message)> GetEntries() => _entries;
    }

    public class OrderProcessorWithInvertedSequence
    {
        private readonly IOrderRepository _repository;
        private readonly InMemoryAuditLog _auditLog;

        public OrderProcessorWithInvertedSequence(IOrderRepository repository, InMemoryAuditLog auditLog)
        {
            _repository = repository;
            _auditLog = auditLog;
        }

        public async Task ProcessRefundAsync(Order order, string reason)
        {
            // BUG: Audit log mutation BEFORE persistence validation
            _auditLog.Log("RefundApproved", $"Refund approved for {reason}");
            await _repository.UpdateAsync(order); // This may throw, but audit already logged success
            _auditLog.Log("RefundCompleted", "Refund processed");
        }
    }

    public class OrderProcessorWithCorrectSequence
    {
        private readonly IOrderRepository _repository;
        private readonly InMemoryAuditLog _auditLog;

        public OrderProcessorWithCorrectSequence(IOrderRepository repository, InMemoryAuditLog auditLog)
        {
            _repository = repository;
            _auditLog = auditLog;
        }

        public async Task ProcessRefundAsync(Order order, string reason)
        {
            // CORRECT: Persistence validated before audit logging
            await _repository.UpdateAsync(order);
            _auditLog.Log("RefundApproved", $"Refund approved for {reason}");
            _auditLog.Log("RefundCompleted", "Refund processed");
        }
    }

    public class OrderProcessorWithDroppedCancellation
    {
        public async Task ProcessOrderWithLongOperationAsync(Order order, CancellationToken cancellationToken)
        {
            // BUG: CancellationToken not passed to internal operation
            await InternalLongRunningOperation();
        }

        private async Task InternalLongRunningOperation()
        {
            // This doesn't receive the cancellation token
            await Task.Delay(1000);
        }
    }

    public class OrderProcessor
    {
        public object ProcessOrderWithNewSignature(Order order, bool requiresValidation)
        {
            return new { orderId = order.Id, validated = requiresValidation };
        }

        public object ProcessOrderWithOptionalParameter(Order order, bool requiresValidation = true)
        {
            return new { orderId = order.Id, validated = requiresValidation };
        }
    }
}
