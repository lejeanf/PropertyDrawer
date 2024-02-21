This package implements a simple property drawer. It helps hiding certains property from the Unity inspector using conditions without having to code a custom editor script every time.


[FEATURES]
- Ability to display the fields of a Scriptable Object within the Unity editor bellow its reference. To enable this feature use the tag [ScriptableObjectDrawer]
 before the declaration of your scriptable object:
'[ScriptableObjectDrawer] public class ClassName : ScriptableObject {}'
- Ability to display a private property as ReadOnly, to Enable this feature use the tag [ReadOnly]
- Ability to Validate the setup of a component using IValidatable interface, the tag [Validation("Explain here what is required")] and a validation function OnValidate(){} in which the IsValid bool is set according to your validation needs.
    

- 


In order to find this package in unity's package manager make sure to add the scoped registery to unity's ProjectSettings:
- click new scopedRegisteries (+) in ProjectSettings/Package manager
- set the following parameters:
	- name: jeanf
	- url: https://registry.npmjs.com
	- scope fr.jeanf

Usage:

```
public bool myBool = false;
[DrawIf("myBool", false, ComparisonType.Equals, DisablingType.DontDraw)]
[SerializeField] public Transform myTransform; 
```


Credits:
- some of the code in this repository was found <a href="https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/">here</a> I then tweaked it to my likings.

