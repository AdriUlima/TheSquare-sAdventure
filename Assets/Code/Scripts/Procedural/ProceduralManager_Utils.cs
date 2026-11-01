using System;
using UnityEngine;
using Random = System.Random;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Rendering;
#endif

public abstract class ProceduralUtil
{
    [SerializeField] protected ushort min, max;

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ProceduralUtil))]
    public class ProceduralUtil_Drawer : PropertyDrawer
    {
        private SerializedProperty property;
        protected float x, y, height;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            this.property = property;
            EditorGUI.BeginProperty(position, label, property);
            x = position.x;
            y = position.y;
            height = EditorGUIUtility.singleLineHeight;
        }

        protected void PropertyFieldInt(SerializedProperty intprop, string label = "")
        {
            if (label != string.Empty)
            {
                EditorGUI.LabelField(new Rect(x, y, 45f, height), label);
                x += 45f;
            }
            intprop.intValue = EditorGUI.IntSlider(new Rect(x, y, 45f, height), intprop.intValue, property.FindPropertyRelative("min").intValue, property.FindPropertyRelative("max").intValue);
            x += 50f;
        }
        protected void PropertyFieldFloat(SerializedProperty floatprop, string label = "")
        {
            if (label != string.Empty)
            {
                EditorGUI.LabelField(new Rect(x, y, 60f, height), label);
                x += 65f;
            }
            floatprop.floatValue = EditorGUI.FloatField(new Rect(x, y, 45f, height), floatprop.floatValue);
            x += 50f;
        }
        protected void PropertyFieldBool(SerializedProperty boolprop, string label)
        {
            boolprop.boolValue = EditorGUI.ToggleLeft(new Rect(x, y, 55f, height), label, boolprop.boolValue);
        }
        protected void PropertyFieldEnum<T>(SerializedProperty enumprop, string label = "") where T : Enum
        {
            if (label != string.Empty)
            {
                EditorGUI.LabelField(new Rect(x, y, 65f, height), label);
                x += 80f;
            }
            enumprop.SetEnumValue(EditorGUI.EnumPopup(new Rect(x, y, 145f, height), enumprop.GetEnumValue<T>()));
            x += 150f;
        }
    }
#endif
}

[Serializable]
public class MinMaxSetting : ProceduralUtil
{
    public string name;
    public ushort value, extraV;
    public bool extraB;

    public MinMaxSetting(string name, ushort value, ushort min, ushort max)
    {
        this.name = name;
        this.value = value;
        extraV = v;
        this.min = min;
        this.max = max;
    }

    private ushort v => (ushort)(value + 1);
    public void Validate() => extraV = Math.Max(v, extraV);

    public ushort GetValueSetting() => extraB ? (ushort)new Random().Next(value, extraV + 1) : value;

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MinMaxSetting))]
    public class MinMaxSetting_Drawer : ProceduralUtil_Drawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);

            PropertyFieldInt(property.FindPropertyRelative("value"), property.FindPropertyRelative("name").stringValue);
            SerializedProperty boolProp = property.FindPropertyRelative("extraB");
            if (boolProp.boolValue) PropertyFieldInt(property.FindPropertyRelative("extraV"));
            PropertyFieldBool(boolProp, "Range");
            EditorGUI.EndProperty();
        }
    }
#endif
}

[Serializable]
public class RandomDistribution : ProceduralUtil
{
    public enum Distribution
    {
        Uniform,
        Normal,
        Exponential,
        Logarithmic
    }
    public Distribution dbution;
    public float p1 = 0.5f, p2 = 1f;
    public bool useSeed = false;
    public int seed;

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(RandomDistribution))]
    public class RandomDistribution_Drawer : ProceduralUtil_Drawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight * 2 + 2f;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);

            SerializedProperty dbution = property.FindPropertyRelative("dbution");
            PropertyFieldEnum<Distribution>(dbution, "Distribution");
            x += 20f;
            SerializedProperty boolProp = property.FindPropertyRelative("useSeed");
            if (boolProp.boolValue) PropertyFieldInt(property.FindPropertyRelative("seed"));
            PropertyFieldBool(boolProp, "Seed");
            x = position.x;
            y += height;

            Distribution aux = (Distribution)dbution.enumValueIndex;
            PropertyFieldFloat(property.FindPropertyRelative("p1"), aux switch
            {
                Distribution.Uniform => "Threshold",
                Distribution.Normal => "Mean",
                Distribution.Exponential => "Lambda",
                Distribution.Logarithmic => "Strength",
                _ => throw new NotImplementedException()
            });
            if (aux is Distribution.Normal) PropertyFieldFloat(property.FindPropertyRelative("p2"), "StdDev");
            EditorGUI.EndProperty();
        }
    }
#endif
}

public class RandomProcedural
{
    public readonly RandomDistribution d;
    private readonly Random rng;

    public RandomProcedural(RandomDistribution rd)
    {
        d = rd;
        rng = d.useSeed ? new Random(d.seed) : new Random();
    }

    public int Range(int min, int max) => rng.Next(min, max); //max excluyente
    public float Range(float min, float max) => (float)(min + rng.NextDouble() * (max - min));

    public bool Bool()
    {
        double n = rng.NextDouble();

        switch (d.dbution)
        {
            case RandomDistribution.Distribution.Uniform:
                return n <= d.p1;

            case RandomDistribution.Distribution.Normal: // Box–Muller transform
                double u1 = 1.0 - rng.NextDouble(), u2 = 1.0 - rng.NextDouble();
                double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                double normal = d.p1 + z * d.p2; // media + desviación
                return normal > n;

            case RandomDistribution.Distribution.Exponential: // p1 = lambda
                double expProb = 1.0 - Math.Exp(-d.p1);
                return n < expProb;

            case RandomDistribution.Distribution.Logarithmic: // p1 controla la fuerza logarítmica
                double log = Math.Log(1 + d.p1 * n) / Math.Log(1 + d.p1);
                return rng.NextDouble() <= log;

            default:
                return false;
        }
    }
}