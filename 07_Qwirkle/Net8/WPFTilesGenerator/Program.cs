using System.Text;

namespace WPFTilesGenerator;

internal class Program
{
    public enum Shape
    {
        Circle,
        Cross,
        Lozange,
        Square,
        Star,
        Clover,
    }

    public enum Color
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Purple,
    }

    static void Main(string[] args)
    {
        var DrawingBrushes = new Dictionary<Shape, string> {
            { Shape.Clover, """
                <DrawingBrush x:Key="$(shape)$(color)">
                    <DrawingBrush.Drawing>
                        <DrawingGroup>
                            <DrawingGroup.Children>
                                <GeometryDrawing Brush="Black" >
                                    <GeometryDrawing.Geometry>
                                        <RectangleGeometry Rect="0,920,200,200" RadiusX="15" RadiusY="15" />
                                    </GeometryDrawing.Geometry>
                                </GeometryDrawing>
                                <GeometryDrawing Brush="{StaticResource $(color)}" Geometry="m100 942.52a33.137 33.137 0 0 0-33.137 33.137 33.137 33.137 0 0 0 9.7344 23.434 33.137 33.137 0 0 0-0.0273 0.0293 33.137 33.137 0 0 0-23.434-9.7363 33.137 33.137 0 0 0-33.137 33.137 33.137 33.137 0 0 0 33.137 33.137 33.137 33.137 0 0 0 23.434-9.7344 33.137 33.137 0 0 0 0.0293 0.027 33.137 33.137 0 0 0-9.7363 23.434 33.137 33.137 0 0 0 33.137 33.137 33.137 33.137 0 0 0 33.137-33.137 33.137 33.137 0 0 0-9.7363-23.434 33.137 33.137 0 0 0 0.0293-0.027 33.137 33.137 0 0 0 23.434 9.7344 33.137 33.137 0 0 0 33.137-33.137 33.137 33.137 0 0 0-33.137-33.137 33.137 33.137 0 0 0-23.432 9.7344 33.137 33.137 0 0 0-0.0312-0.0274 33.137 33.137 0 0 0 9.7363-23.434 33.137 33.137 0 0 0-33.137-33.137z">
                                    <GeometryDrawing.Pen>
                                        <Pen Brush="#000000" />
                                    </GeometryDrawing.Pen>
                                </GeometryDrawing>
                            </DrawingGroup.Children>
                        </DrawingGroup>
                    </DrawingBrush.Drawing>
                </DrawingBrush>
                """ },

            { Shape.Lozange, """
                <DrawingBrush x:Key="$(shape)$(color)">
                    <DrawingBrush.Drawing>
                        <DrawingGroup Transform="1.0,0.0,0.0,1.0,0.0,-922.52">
                            <GeometryDrawing Brush="Black" >
                                <GeometryDrawing.Geometry>
                                    <RectangleGeometry Rect="0,922,200,200" RadiusX="15" RadiusY="15" />
                                </GeometryDrawing.Geometry>
                            </GeometryDrawing>
                            <DrawingGroup Transform="0.7071067811865476,-0.7071067811865475,0.7071067811865475,0.7071067811865476,0.0,0.0">
                                <GeometryDrawing Brush="{StaticResource $(color)}">
                                    <GeometryDrawing.Geometry>
                                        <RectangleGeometry Rect="-704.0,740.0,103.0,103.0"/>
                                    </GeometryDrawing.Geometry>
                                </GeometryDrawing>
                            </DrawingGroup>
                        </DrawingGroup>
                    </DrawingBrush.Drawing>
                </DrawingBrush>
                """ },

            { Shape.Star, """
                <DrawingBrush x:Key="$(shape)$(color)">
                    <DrawingBrush.Drawing>
                        <DrawingGroup Transform="1.0,0.0,0.0,1.0,0.0,-922.52">
                            <GeometryDrawing Brush="Black" >
                                <GeometryDrawing.Geometry>
                                    <RectangleGeometry Rect="0,922,200,200" RadiusX="15" RadiusY="15" />
                                </GeometryDrawing.Geometry>
                            </GeometryDrawing>
                            <GeometryDrawing Brush="{StaticResource $(color)}">
                                <GeometryDrawing.Geometry>
                                    <PathGeometry Figures="m 100 942.52 l -12.473 49.889 l -44.096 -26.457 l 26.457 44.096 l -49.889 12.473 l 49.889 12.473 l -26.457 44.096 l 44.096 -26.457 l 12.473 49.889 l 12.473 -49.889 l 44.096 26.457 l -26.457 -44.096 l 49.889 -12.473 l -49.889 -12.473 l 26.457 -44.096 l -44.096 26.457 z" FillRule="Nonzero"/>
                                </GeometryDrawing.Geometry>
                            </GeometryDrawing>
                        </DrawingGroup>
                    </DrawingBrush.Drawing>
                </DrawingBrush>
                """ },

            { Shape.Circle, """
                <DrawingBrush x:Key="$(shape)$(color)">
                    <DrawingBrush.Drawing>
                        <DrawingGroup Transform="1.0,0.0,0.0,1.0,0.0,-922.52">
                            <GeometryDrawing Brush="Black" >
                                <GeometryDrawing.Geometry>
                                    <RectangleGeometry Rect="0,922,200,200" RadiusX="15" RadiusY="15" />
                                </GeometryDrawing.Geometry>
                            </GeometryDrawing>
                            <GeometryDrawing Brush="{StaticResource $(color)}">
                                <GeometryDrawing.Geometry>
                                    <EllipseGeometry RadiusX="70.0" RadiusY="70.0" Center="100.0,1022.5"/>
                                </GeometryDrawing.Geometry>
                            </GeometryDrawing>
                        </DrawingGroup>
                    </DrawingBrush.Drawing>
                </DrawingBrush>
                """ },

            { Shape.Square, """
                <DrawingBrush x:Key="$(shape)$(color)">
                    <DrawingBrush.Drawing>
                        <DrawingGroup Transform="1.0,0.0,0.0,1.0,0.0,-922.52">
                            <GeometryDrawing Brush="Black" >
                                <GeometryDrawing.Geometry>
                                    <RectangleGeometry Rect="0,922,200,200" RadiusX="15" RadiusY="15" />
                                </GeometryDrawing.Geometry>
                            </GeometryDrawing>
                            <GeometryDrawing Brush="{StaticResource $(color)}">
                                <GeometryDrawing.Geometry>
                                    <RectangleGeometry Rect="35.0,957.52,130.0,130.0"/>
                                </GeometryDrawing.Geometry>
                            </GeometryDrawing>
                        </DrawingGroup>
                    </DrawingBrush.Drawing>
                </DrawingBrush>
                """ },

            { Shape.Cross, """
                <DrawingBrush x:Key="$(shape)$(color)">
                    <DrawingBrush.Drawing>
                        <DrawingGroup Transform="1.0,0.0,0.0,1.0,0.0,-922.52">
                            <GeometryDrawing Brush="Black" >
                                <GeometryDrawing.Geometry>
                                    <RectangleGeometry Rect="0,922,200,200" RadiusX="15" RadiusY="15" />
                                </GeometryDrawing.Geometry>
                            </GeometryDrawing>
                            <GeometryDrawing Brush="{StaticResource $(color)}">
                                <GeometryDrawing.Geometry>
                                    <PathGeometry Figures="m 30.4591 954.473 l 42.8004 70.288 l -42.8004 70.2881 l 71.3339 -42.1728 l 71.334 42.1728 l -42.8004 -70.2881 l 42.8004 -70.288 l -71.334 42.1728 z" FillRule="Nonzero"/>
                                </GeometryDrawing.Geometry>
                            </GeometryDrawing>
                        </DrawingGroup>
                    </DrawingBrush.Drawing>
                </DrawingBrush>
                """ }, };

        using var sw = new StreamWriter(@"c:\temp\tiles.xaml", false, Encoding.UTF8);
        var sb = new StringBuilder();
        int row = 0;
        foreach (Shape shape in Enum.GetValues(typeof(Shape)))
        {
            int col = 0;
            foreach (Color color in Enum.GetValues(typeof(Color)))
            {
                var s = DrawingBrushes[shape].Replace("$(shape)", shape.ToString()).Replace("$(color)", color.ToString());
                sw.WriteLine(s+"\n");
                sb.AppendLine($"<Rectangle Grid.Row=\"{row}\" Grid.Column=\"{col}\" Margin=\"1\" Width=\"150\" Height=\"150\" Fill=\"{{StaticResource {shape}{color}}}\" />");
                col++;
            }
            row++;
        }
        sw.WriteLine("\r\n\r\n");
        sw.WriteLine(sb.ToString());
    }
}
