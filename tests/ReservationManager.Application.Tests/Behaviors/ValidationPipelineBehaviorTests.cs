using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using ReservationManager.Application.Behaviors;

namespace ReservationManager.Application.Tests.Behaviors;

public record TestPipelineRequest(string Value) : IRequest<string>;

public class ValidationPipelineBehaviorTests
{
    private class PassingValidator : AbstractValidator<TestPipelineRequest> { }

    private class FailingValidator : AbstractValidator<TestPipelineRequest>
    {
        public FailingValidator()
        {
            RuleFor(x => x.Value).NotEmpty().WithMessage("Value is required");
        }
    }

    [Fact]
    public async Task NoValidators_CallsNext()
    {
        var validators = Enumerable.Empty<IValidator<TestPipelineRequest>>();
        var behavior = new ValidationPipelineBehavior<TestPipelineRequest, string>(validators);

        var nextCalled = false;
        RequestHandlerDelegate<string> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        };

        var result = await behavior.Handle(new TestPipelineRequest("test"), next, CancellationToken.None);

        result.Should().Be("ok");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatorsPass_CallsNext()
    {
        var behavior = new ValidationPipelineBehavior<TestPipelineRequest, string>(
            new[] { new PassingValidator() });

        var nextCalled = false;
        RequestHandlerDelegate<string> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        };

        var result = await behavior.Handle(new TestPipelineRequest("test"), next, CancellationToken.None);

        result.Should().Be("ok");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatorFails_ThrowsValidationException_DoesNotCallNext()
    {
        var behavior = new ValidationPipelineBehavior<TestPipelineRequest, string>(
            new[] { new FailingValidator() });

        var nextCalled = false;
        RequestHandlerDelegate<string> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        };

        var act = () => behavior.Handle(new TestPipelineRequest(""), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        nextCalled.Should().BeFalse();
    }
}
