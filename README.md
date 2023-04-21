This package implements a simple property drawer. It helps hiding certains property from the Unity inspector using conditions without having to code a custom editor script every time.


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
Or-Aviram (code found <a href="https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/">here</a>)
I simply implemented this code in my package registry and tweaked it to my liking.
