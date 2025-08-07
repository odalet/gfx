using System;
using System.Collections.Generic;
using System.Linq;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using ScanPlayerWpf.Models;
using SharpDX;

namespace ScanPlayerWpf.Rendering
{
    public sealed class SceneRenderer : ISceneRenderer
    {
        private const float heightEpsilon = 0.001f;

        public SceneRenderer(SceneNodeGroupModel3D target) => Target = target;

        private IScene Scene { get; set; }
        private SceneNodeGroupModel3D Target { get; }

        public void Render(double surfaceWidth, double surfaceHeight, IScene scene)
        {
            Scene = scene ?? throw new ArgumentNullException(nameof(scene));

            Target.Clear();

            DrawReference();
            DrawPlatform();
            DrawHeadReferences();
            DrawHeadFields();

            if (Scene.Program != null)
            {
                var interpreter = new SceneInterpreter(TimeSpan.MaxValue, Scene, Target);
                interpreter.Execute();
            }
        }

        private void DrawReference()
        {
            const float baseLength = 25f;
            const float z = heightEpsilon;
            var builder = new LineBuilder();

            void addLine(Vector3 p1, Vector3 p2) => builder.AddLine(
                new Vector3(p1.X, p1.Y, p1.Z + z), new Vector3(p2.X, p2.Y, p2.Z + z));

            addLine(Vector3.Zero, Vector3.UnitX * baseLength);
            var xaxis = new LineNode
            {
                Name = $"{NodeNames.Reference}X",
                Geometry = builder.ToLineGeometry3D(),
                Material = new LineMaterialCore
                {
                    LineColor = new Color4(1f, 0f, 0f, 1f),
                    Thickness = 1f
                }
            };

            builder = new LineBuilder();
            addLine(Vector3.Zero, Vector3.UnitY * baseLength);
            var yaxis = new LineNode
            {
                Name = $"{NodeNames.Reference}Y",
                Geometry = builder.ToLineGeometry3D(),
                Material = new LineMaterialCore
                {
                    LineColor = new Color4(0f, 1f, 0f, 1f),
                    Thickness = 1f
                }
            };

            builder = new LineBuilder();
            addLine(Vector3.Zero, Vector3.UnitZ * baseLength);
            var zaxis = new LineNode
            {
                Name = $"{NodeNames.Reference}Z",
                Geometry = builder.ToLineGeometry3D(),
                Material = new LineMaterialCore
                {
                    LineColor = new Color4(0f, 0f, 1f, 1f),
                    Thickness = 1f
                }
            };

            var reference = new GroupNode { Name = NodeNames.Reference, Visible = false };
            _ = reference.AddChildNode(xaxis);
            _ = reference.AddChildNode(yaxis);
            _ = reference.AddChildNode(zaxis);

            Target.AddNode(reference);
        }

        private void DrawPlatform()
        {
            var platform = Scene.Printer.BuildZone;
            var radius = platform.CornerRadius;
            const float z = 0f;

            var path = new List<Vector3>();
            path.Add(new Vector3((float)(platform.XMin + radius), (float)platform.YMin, z));
            path.Add(new Vector3((float)(platform.XMax - radius), (float)platform.YMin, z));
            AddArc(path, platform.XMax - radius, platform.YMin + radius, radius, -Math.PI / 2, Math.PI / 2, z);

            path.Add(new Vector3((float)platform.XMax, (float)(platform.YMin + radius), z));
            path.Add(new Vector3((float)platform.XMax, (float)(platform.YMax - radius), z));
            AddArc(path, platform.XMax - radius, platform.YMax - radius, radius, 0.0, Math.PI / 2, z);

            path.Add(new Vector3((float)(platform.XMax - radius), (float)platform.YMax, z));
            path.Add(new Vector3((float)(platform.XMin + radius), (float)platform.YMax, z));
            AddArc(path, platform.XMin + radius, platform.YMax - radius, radius, Math.PI / 2, Math.PI / 2, z);

            path.Add(new Vector3((float)platform.XMin, (float)(platform.YMax - radius), z));
            path.Add(new Vector3((float)platform.XMin, (float)(platform.YMin + radius), z));
            AddArc(path, platform.XMin + radius, platform.YMin + radius, radius, Math.PI, Math.PI / 2, z);

            var builder = new MeshBuilder();
            builder.AddTube(path, 1.0, 12, true);
            var meshGeometry = builder.ToMeshGeometry3D();
            var node = new MeshNode
            {
                Name = NodeNames.Platform,
                Visible = false,
                Geometry = meshGeometry,
                Material = PhongMaterials.Gold,
                CullMode = SharpDX.Direct3D11.CullMode.Back
            };

            Target.AddNode(node);
        }

        private void DrawHeadReferences()
        {
            const float baseLength = 20f; // 10
            const float z = heightEpsilon;

            string rotateText(string text, int rotation)
            {
                var r = rotation % 4;                
                if (r == 0) return $"   {text}";
                if (r == 1) return $"{text}\n\n ";
                if (r == 2) return $"{text}   ";
                if (r == 3) return $" \n{text}";
                return text; // Should never happen
            }

            GroupNode makeHeadReference(IHeadDefinition head, Color4 color)
            {
                var builder = new LineBuilder();
                builder.AddLine(Vector3.Zero, Vector3.UnitX * baseLength);
                var xaxis = new LineNode
                {
                    Geometry = builder.ToLineGeometry3D(),
                    Material = new LineMaterialCore
                    {
                        LineColor = color,
                        Thickness = 1f
                    }
                };

                var x = ResourceHelper.Create();
                x.TextInfo.Add(new TextInfo(rotateText("X", head.Rotation), Vector3.UnitX * baseLength) { Foreground = color, Scale = 0.5f });

                builder = new LineBuilder();
                builder.AddLine(Vector3.Zero, Vector3.UnitY * baseLength);
                var yaxis = new LineNode
                {
                    Geometry = builder.ToLineGeometry3D(),
                    Material = new LineMaterialCore
                    {
                        LineColor = color,
                        Thickness = 1f
                    }
                };

                var y = ResourceHelper.Create();
                y.TextInfo.Add(new TextInfo(rotateText("Y", head.Rotation + 1), Vector3.UnitY * baseLength) { Foreground = color, Scale = 0.5f });

                var group = new GroupNode { Name = NodeNames.GetHeadNodeName(head) };
                _ = group.AddChildNode(xaxis);
                _ = group.AddChildNode(new BillboardNode
                {
                    Geometry = x,
                    Material = new BillboardMaterialCore
                    {
                        FixedSize = true,
                        Type = BillboardType.SingleText
                    }
                });

                _ = group.AddChildNode(yaxis);
                _ = group.AddChildNode(new BillboardNode
                {
                    Geometry = y,
                    Material = new BillboardMaterialCore
                    {
                        FixedSize = true,
                        Type = BillboardType.SingleText
                    }
                });

                var t = Matrix.Translation((float)head.X, (float)head.Y, z);
                var r = Matrix.RotationZ((float)(head.Rotation * Math.PI / 2.0));
                group.ModelMatrix = r * t;

                var ss = new ScreenSpacedNode();
                _ = ss.AddChildNode(group);

                return ss;
            }

            var headReferences = new GroupNode { Name = NodeNames.HeadReferences, Visible = false };

            foreach (var head in Scene.Printer.Heads)
            {
                var color = Palette.GetColor4(head.PreferredColorIndex, PaletteStyle.Light);
                var headNode = makeHeadReference(head, color);
                _ = headReferences.AddChildNode(headNode);
            }

            Target.AddNode(headReferences);
        }

        private void DrawHeadFields()
        {
            const float z = -2f * heightEpsilon;

            GroupNode makeHeadField(IHeadDefinition head)
            {
                var builder = new MeshBuilder();
                builder.AddBox(Vector3.Zero, (float)head.UsableField.Width, (float)head.UsableField.Height, heightEpsilon / 10f);
                var field = new MeshNode
                {
                    Geometry = builder.ToMeshGeometry3D(),
                    Material = new DiffuseMaterialCore
                    {
                        DiffuseColor = Palette.GetColor4(head.PreferredColorIndex, 0.125f)
                    }
                };

                var group = new GroupNode { Name = NodeNames.GetHeadNodeName(head) };
                _ = group.AddChildNode(field);

                var t = Matrix.Translation((float)head.X, (float)head.Y, z);
                var r = Matrix.RotationZ((float)(head.Rotation * Math.PI / 2.0));
                group.ModelMatrix = r * t;

                return group;
            }

            var headFields = new GroupNode { Name = NodeNames.HeadFields, Visible = false };

            foreach (var head in Scene.Printer.Heads)
            {
                var headNode = makeHeadField(head);
                _ = headFields.AddChildNode(headNode);
            }

            Target.AddNode(headFields);
        }

        private void AddArc(List<Vector3> path, double ox, double oy, double r, double startAngle, double arcAngle, double z) => path.AddRange(
            Utils.GetArc(ox, oy, r, startAngle, arcAngle, 8, z)
            .Select(p => new Vector3((float)p.x, (float)p.y, (float)p.z)));
    }
}
