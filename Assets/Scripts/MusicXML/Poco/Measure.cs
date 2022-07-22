
public struct Measure
{
    public MeasureAttribute? Attribute { get; set; }
    public long Number { get; set; }
    public IMeasureChild[] Children { get; set; }
}