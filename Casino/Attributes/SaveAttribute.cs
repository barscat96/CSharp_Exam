namespace CSharp_Exam.Casino.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class SaveAttribute(string class_name) : Attribute
{
    public readonly string ClassName = class_name;
}