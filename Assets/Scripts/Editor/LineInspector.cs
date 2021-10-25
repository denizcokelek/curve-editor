using System;
using UnityEditor;
using UnityEngine;

namespace Project.Editor
{
    [CustomEditor(typeof(Line))]
    public class LineInspector : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            Line line = target as Line;

            Transform lineTransform = line.transform;
            Quaternion lineRotation = lineTransform.rotation;
            //Quaternion lineRotation = Tools.pivotRotation == PivotRotation.Local ? lineTransform.rotation : Quaternion.identity; //??????
            Vector3 point0 = lineTransform.TransformPoint(line.Point0);
            Vector3 point1 = lineTransform.TransformPoint(line.Point1);
            
            Handles.color = Color.white;
            Handles.DrawLine(point0, point1);

            Handles.DoPositionHandle(point0, lineRotation);
            Handles.DoPositionHandle(point1, lineRotation);
                
            line.Point0 = lineTransform.InverseTransformPoint(point0);
            line.Point1 = lineTransform.InverseTransformPoint(point1);

            //EditorGUI.BeginChangeCheck();
            //
            //if (EditorGUI.EndChangeCheck())
            //{
            //    line.Point0 = lineTransform.InverseTransformPoint(point0);
            //}
            //EditorGUI.BeginChangeCheck();
            //Handles.DoPositionHandle(point1, lineRotation);
            //if (EditorGUI.EndChangeCheck())
            //{
            //    line.Point1 = lineTransform.InverseTransformPoint(point1);
            //}
        }
    }
}
