using System;
using System.Collections.Generic;
using System.Linq;
using AddUp.Geometry;
using AddUp.Geometry.Shapes;
using AddUp.NCore.Printing;
using AddUp.NCore.Printing.Catalog;

namespace ScanPlayer.Models;

internal interface IPrinterCharacteristics
{
    BuildVolume NominalBuildVolume { get; }
    BuildVolume ActualBuildVolume { get; }
    IReadOnlyList<HeadCharacteristics> Heads { get; }
}

internal sealed class PrinterCharacteristics : IPrinterCharacteristics
{
    private readonly List<HeadCharacteristics> heads = new();

    public PrinterCharacteristics(IPrinterDefinition printerDefinition)
    {
        PrinterDefinition = printerDefinition ?? throw new ArgumentNullException(nameof(printerDefinition));

        InitializeBuildVolume();
        InitializeHeads();
    }

    public BuildVolume NominalBuildVolume { get; private set; }
    public BuildVolume ActualBuildVolume { get; private set; }
    public IReadOnlyList<HeadCharacteristics> Heads => heads;
    private IPrinterDefinition PrinterDefinition { get; }

    private void InitializeBuildVolume()
    {
        NominalBuildVolume = PrinterDefinition.Model.NominalBuildVolume;
        ActualBuildVolume = PrinterDefinition.Model.ActualBuildVolume;
    }

    private void InitializeHeads()
    {
        double orientationToDegrees(HeadOrientation orientation) =>
            AngleUtils.QuarterTurnsToDegrees(AngleUtils.NormalizeQuarterTurns((int)orientation));

        FieldBounds shapeToFieldDefinition(IShape2D shape) => new(
            shape.Bounds.Min.X, shape.Bounds.Min.Y, shape.Bounds.Max.X, shape.Bounds.Max.Y);

        var headsProperties = PrinterDefinition.ScanningDeviceDefinitions.Values.Select(x => new
        {
            x.Id,
            CenterX = x.Mounting.X,
            CenterY = x.Mounting.Y,
            Rotation = orientationToDegrees(x.Mounting.Orientation),
            x.TargetField.PlatformToHeadTransform,
            x.MaxField
        });

        var index = 0;
        foreach (var head in headsProperties)
        {
            var maxField = shapeToFieldDefinition(head.MaxField);
            var targetField = shapeToFieldDefinition(ComputeTargetField(head.PlatformToHeadTransform, head.MaxField));
            var characteristics = new HeadCharacteristics(
                head.Id, head.CenterX, head.CenterY, head.Rotation, index++, maxField, targetField);
            heads.Add(characteristics);
        }
    }

    private IShape2D ComputeTargetField(IPlatformToHeadTransform transform, IShape2D maxField)
    {
        var transformedPlatform = transform.Apply(ActualBuildVolume.Platform);
        return IntersectShape.Create(maxField.Center, maxField, transformedPlatform);
    }
}
