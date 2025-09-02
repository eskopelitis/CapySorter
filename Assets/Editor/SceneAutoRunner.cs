namespace Capy.Editor
{
    public static class SceneAutoRunner
    {
        // Called via Unity CLI: -executeMethod Capy.Editor.SceneAutoRunner.BuildGrayboxSceneCLI
        public static void BuildGrayboxSceneCLI()
        {
            SceneAutoBuilder.BuildGrayboxScene();
        }
    }
}
