using System.Collections.Generic;
using AddUp.Geometry;
using AddUp.Geometry.Shapes;

namespace ScanPlayerWpf.Models
{
    public interface IPrinterDefinition
    {
        string Name { get; }
        PrinterBuildZone BuildZone { get; }
        IReadOnlyList<IHeadDefinition> Heads { get; }
    }

    public interface IHeadDefinition
    {
        string Name { get; }
        int Id { get; }
        double X { get; }
        double Y { get; }
        int Rotation { get; } // in quarter turns
        Rectangle UsableField { get; }
        IShape2D TargetField { get; }
        int PreferredColorIndex { get; }
    }

    internal class HeadDefinition : IHeadDefinition
    {
        public HeadDefinition(int id, string name, double x, double y, int rotation, Rectangle usableField, IShape2D printerField)
        {
            Id = id;
            Name = name;
            X = x;
            Y = y;
            Rotation = rotation;
            UsableField = usableField;
            TargetField = ComputeTargetField(usableField, printerField);
        }

        public int Id { get; }
        public string Name { get; }      
        public double X { get; }
        public double Y { get; }
        public int Rotation { get; }
        public Rectangle UsableField { get; }
        public IShape2D TargetField { get; }
        public int PreferredColorIndex { get; set; }

        // Result in the head reference
        public IShape2D ComputeTargetField(IShape2D usableField, IShape2D printerField) 
        {
            // h2p: Head to Platform Transform is T°R
            var h2pRotation = Rotation;
            var h2pTranslation = new Vector2D(X, Y);

            var p2hRotation = -h2pRotation; // inverse rotation
            var p2hTranslation = new Vector2D(-h2pTranslation.X, -h2pTranslation.Y); // Inverse translation(still in the platform orientation) 

            var headField = usableField;
            var platform = printerField.Translate(p2hTranslation).RotateAround(Vector2D.Zero, p2hRotation);

            return IntersectShape.Create(headField.Center, headField, platform);
        }
    }
}
