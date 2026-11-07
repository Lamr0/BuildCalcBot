namespace calcBot;

public enum Wall { Half, One, OneAndHalf }

public static class Calc
{
    public const int BricksPerM3 = 394;
    public const double Reserve = 1.05;

    
    public static double Thickness(Wall w) => w switch
    {
        Wall.Half => 0.12,
        Wall.One => 0.25,
        _ => 0.38
    };
    
    public static double Volume(Wall w, double length, double height) =>
        length * height * Thickness(w);
    
    public static int Bricks(double volume) =>
        (int)Math.Ceiling(volume * BricksPerM3);
    
    public static int BricksWithReserve(double volume) =>
        (int)Math.Ceiling(volume * BricksPerM3 * Reserve);
}