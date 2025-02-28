#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace jeanf.validationTools
{
    public class SceneValidationOnBuild : IProcessSceneWithReport
    {
        public int callbackOrder => 0;
        
        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if(!BuildPipeline.isBuildingPlayer) return;

            var statusMessage = AreSceneValidationComponentsValid(scene);
            if (!statusMessage.IsValid)
            {
                throw  new BuildFailedException($"The scene {scene.name} has failed validation. errors: {string.Join("||| ", statusMessage.ErrorMessages)}");
            }
        }

        private static StatusMessage AreSceneValidationComponentsValid(Scene scene)
        {
            var statusMessage = new StatusMessage(isValid: true, null);
            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                var validatables = rootGameObject.GetComponentsInChildren<IValidatable>();
                if (validatables is not { Length: > 0 }) continue;
                foreach (var validatable in validatables)
                {
                    if (!validatable.IsValid)
                    {
                        statusMessage.IsValid = false;
                        statusMessage.ErrorMessages.Add($"class {validatable.GetType().Name} has failed validation. validatable: [{validatable}]");
                    }
                }
            }
            return statusMessage;
        }

        private struct StatusMessage
        {
            public bool IsValid;
            public List<string> ErrorMessages;

            public StatusMessage(bool isValid, List<string> errorMessages)
            {
                this.IsValid = isValid;
                this.ErrorMessages = errorMessages;
            }
        }
    }
}

#endif