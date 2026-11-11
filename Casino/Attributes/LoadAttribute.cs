namespace CSharp_Exam.Casino.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class LoadAttribute(string class_name) : Attribute
{
    public readonly string ClassName = class_name;
}