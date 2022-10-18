namespace Min.DependencyInjection;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class IgnoreInjectionAttribute : Attribute
{
    public bool Inherit { get; set; }

    public IgnoreInjectionAttribute(bool inherit = false)
    {
        Inherit = inherit;
    }
}
