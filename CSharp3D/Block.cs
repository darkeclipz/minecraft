namespace CSharp3D;

public partial struct Block
{
    public BlockType Type;
    public int LightLevel;
    
    public static Block LitAir = new Block { Type = BlockType.Air, LightLevel = 15 };
    public static Block UnlitAir = new Block { Type = BlockType.Air, LightLevel = 0 };
}