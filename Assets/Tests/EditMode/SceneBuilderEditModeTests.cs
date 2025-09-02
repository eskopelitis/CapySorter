using NUnit.Framework;

public class SceneBuilderEditModeTests
{
    [Test]
    public void Scene_Builder_Saves_GameScene()
    {
        // Ensure the graybox scene is created on disk (Editor-only)
        SceneAutoBuilder.BuildGrayboxScene();
        Assert.IsTrue(System.IO.File.Exists("Assets/_Project/Scenes/GameScene.unity"));
    }
}
