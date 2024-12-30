namespace CSharp3D;

public struct Block
{
    public BlockType Type;
    public int LightLevel;
    
    public static Block LitAir = new Block { Type = BlockType.Air, LightLevel = 15 };
    public static Block UnlitAir = new Block { Type = BlockType.Air, LightLevel = 0 };

    public static bool IsSolid(BlockType type) => type switch
    {
        BlockType.Air => false,
        BlockType.Flower1 => false,
        BlockType.Flower2 => false,
        BlockType.Flower3 => false,
        BlockType.Flower4 => false,
        BlockType.BigPlantBottom => false,
        BlockType.BigPlantTop => false,
        _ => true
    };

    public static bool IsPlant(BlockType type) => type switch
    {
        BlockType.Flower1 => true,
        BlockType.Flower2 => true,
        BlockType.Flower3 => true,
        BlockType.Flower4 => true,
        BlockType.BigPlantBottom => true,
        BlockType.BigPlantTop => true,
        _ => false
    };
}