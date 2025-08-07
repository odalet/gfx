namespace ScanPlayer.Models;

internal readonly record struct FieldBounds(
    double XMin, double YMin, double XMax, double YMax);

internal readonly record struct HeadCharacteristics(
    int Id,
    double CenterX,
    double CenterY,
    double Rotation,
    int ColorIndex,
    FieldBounds MaxField,
    FieldBounds TargetField)
{
    public string Name { get; } = $"Head_{Id}";
    public (double x, double y) Center => (CenterX, CenterY);
}