#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;

namespace jeanf.validationTools
{
    public class SceneValidationOnBuild : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if(!BuildPipeline.isBuildingPlayer) return;

            if (!AreSceneValidationComponentsValid(scene))
            {
                throw  new BuildFailedException($"The scene {scene.name} has failed validation.");
            }
        }

        private static bool AreSceneValidationComponentsValid(Scene scene)
        {
            var isValid = true;
            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                var validatables = rootGameObject.GetComponentsInChildren<IValidatable>();
                if (validatables is not { Length: > 0 }) continue;
                foreach (var validatable in validatables)
                {
                    if (!validatable.IsValid) isValid = false;
                }
            }
            return isValid;
        }
    }
}

#endif