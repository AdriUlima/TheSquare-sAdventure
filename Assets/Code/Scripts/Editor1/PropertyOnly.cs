/*using Sirenix.OdinInspector;
using System;

[IncludeMyAttributes]
[Required, ChildGameObjectsOnly]
public class RequiredChildAttribute : Attribute
{

}*/

using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class RequiredChildAttribute : PropertyAttribute
{
    // Este atributo solo sirve como "etiqueta" para marcar tus variables
}