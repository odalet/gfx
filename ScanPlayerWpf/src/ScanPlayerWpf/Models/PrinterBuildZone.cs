using System;
using System.Globalization;
using AddUp.Geometry.Shapes;

namespace ScanPlayerWpf.Models
{
    /// <summary>
    /// Stores all the values needed to represent a printer's build zone
    /// </summary>
    /// <seealso cref="System.IEquatable{AddUp.NCore.Printing.BuildZoneDefinition}" />
    public readonly struct PrinterBuildZone : IEquatable<PrinterBuildZone>
    {
        public static PrinterBuildZone CreateCylinder(double radius, double height) =>
            new PrinterBuildZone(radius * 2.0, radius * 2.0, height, radius);

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildZoneDefinition"/> structure.
        /// </summary>
        /// <param name="width">The width in mm (X).</param>
        /// <param name="depth">The depth in mm (Y).</param>
        /// <param name="height">The height in mm (Z).</param>
        /// <param name="radius">The corner radius in degrees.</param>
        public PrinterBuildZone(double width, double depth, double height, double radius)
        {
            Width = width;
            Depth = depth;
            Height = height;
            CornerRadius = radius;
        }

        public static PrinterBuildZone InfiniteBuildZone { get; } =
            new PrinterBuildZone(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, 0.0);

        /// <summary>Gets the width in mm (X). </summary>
        public double Width { get; }

        /// <summary>Gets the depth in mm (Y). </summary>
        public double Depth { get; }

        /// <summary>Gets the height in mm (Z). </summary>
        public double Height { get; }

        /// <summary>Gets the corner radius in degrees. </summary>
        public double CornerRadius { get; }

        /// <summary>
        /// Gets the lower x coordinate of this zone's bounding box in mm.
        /// </summary>
        public double XMin => -Width / 2.0;

        /// <summary>
        /// Gets the higher x coordinate of this zone's bounding box in mm.
        /// </summary>
        public double XMax => Width / 2.0;

        /// <summary>
        /// Gets the lower y coordinate of this zone's bounding box in mm.
        /// </summary>
        public double YMin => -Depth / 2.0;

        /// <summary>
        /// Gets the higher y coordinate of this zone's bounding box in mm.
        /// </summary>
        public double YMax => Depth / 2.0;

        /// <summary>
        /// Gets the lower z coordinate of this zone's bounding box in mm.
        /// </summary>
        public double ZMin => 0.0;

        /// <summary>
        /// Gets the higher z coordinate of this zone's bounding box in mm.
        /// </summary>
        public double ZMax => Height;

        /// <summary>
        /// Gets the definition of the platform as a shape object.
        /// </summary>
        public IShape2D PlatformShape => double.IsPositiveInfinity(Width) && double.IsPositiveInfinity(Height) ?
            (IShape2D)new InfiniteShape() : RoundedRectangle.CreateCentered(Width, Depth, CornerRadius);

        public bool Equals(PrinterBuildZone other) =>
            Width == other.Width &&
            Depth == other.Depth &&
            Height == other.Height &&
            CornerRadius == other.CornerRadius;

        public override bool Equals(object obj) => obj is PrinterBuildZone pbz && Equals(pbz);

        public override int GetHashCode()
        {
            var hashCode = 1348706233;
            hashCode = hashCode * -1521134295 + Width.GetHashCode();
            hashCode = hashCode * -1521134295 + Depth.GetHashCode();
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            hashCode = hashCode * -1521134295 + CornerRadius.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            string _(double d) => d.ToString(CultureInfo.InvariantCulture);
            return $"[{_(Width)}, {_(Depth)}, {_(Height)} - {_(CornerRadius)}]";
        }

        public static bool operator ==(PrinterBuildZone left, PrinterBuildZone right) => left.Equals(right);
        public static bool operator !=(PrinterBuildZone left, PrinterBuildZone right) => !(left == right);
    }
}
