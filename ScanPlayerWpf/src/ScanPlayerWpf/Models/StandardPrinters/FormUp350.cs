using System;
using System.Collections.Generic;
using AddUp.Geometry.Shapes;

namespace ScanPlayerWpf.Models.StandardPrinters
{
    public class FormUp350 : IPrinterDefinition
    {
        private readonly List<IHeadDefinition> heads;
        public FormUp350(int headCount)
        {
            if (headCount < 0 || headCount > 2) throw new ArgumentException(nameof(headCount));

            Name = $"FormUp 350 {headCount}-Laser";
            BuildZone = new PrinterBuildZone(350.0, 350.0, 350.0, 30.0);
            heads = new List<IHeadDefinition>(headCount);
            FillHeads(headCount);
        }

        public string Name { get; }
        public PrinterBuildZone BuildZone { get; }
        public IReadOnlyList<IHeadDefinition> Heads => heads;

        private void FillHeads(int headCount)
        {
            var usableField = Rectangle.CreateCentered(559.0, 559.0);
            var printerField = BuildZone.PlatformShape;
            var colorIndex = 0;

            if (headCount >= 1) heads.Add(
                new HeadDefinition(1, "SMC_1", 0.0, -70.0, 2, usableField, printerField)
                {
                    PreferredColorIndex = colorIndex++
                });

            if (headCount >= 2) heads.Add(
                new HeadDefinition(2, "SMC_2", 0.0, 70.0, 2, usableField, printerField)
                {
                    PreferredColorIndex = colorIndex++
                });
        }
    }
}
