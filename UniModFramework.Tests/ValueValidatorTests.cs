using Xunit;

namespace UniModFramework.Tests;

/// <summary>
/// Tests the untyped IValueValidator.Validate(object?) that loaders and config UIs call.
/// </summary>
public class ValueValidatorTests
{
    [Fact]
    public void Untyped_validate_passes_values_of_the_right_type()
    {
        IValueValidator validator = new RangeValueValidator<float>(3f, 9f);
        Assert.True(validator.Validate(4.5f));
        Assert.False(validator.Validate(10f));
    }

    [Fact]
    public void Untyped_validate_rejects_null_for_value_types()
    {
        IValueValidator validator = new RangeValueValidator<int>(0, 10);
        Assert.False(validator.Validate(null));
    }

    [Fact]
    public void Untyped_validate_rejects_the_wrong_type()
    {
        IValueValidator validator = new FuncValueValidator<int>(_ => true);
        Assert.False(validator.Validate("5"));
    }

    [Fact]
    public void Untyped_validate_passes_null_to_reference_type_validators()
    {
        IValueValidator acceptsNull = new FuncValueValidator<string>(s => s is null);
        IValueValidator listWithoutNull = new ListValueValidator<string>("a", "b");
        Assert.True(acceptsNull.Validate(null));
        Assert.False(listWithoutNull.Validate(null));
        Assert.True(listWithoutNull.Validate("b"));
    }
}
