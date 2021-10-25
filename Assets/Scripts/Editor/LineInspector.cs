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
            Vector3 point1 = lineTransform.TransformPoint(line.Point1);
            Vector3 point2 = lineTransform.TransformPoint(line.Point2);
            
            Handles.color = Color.white;
            Handles.DrawLine(point1, point2);

            Handles.DoPositionHandle(point1, lineRotation);
            Handles.DoPositionHandle(point2, lineRotation);

            line.Point1 = lineTransform.InverseTransformPoint(point1);
            line.Point2 = lineTransform.InverseTransformPoint(point2);

            //EditorGUI.BeginChangeCheck();
            //
            //if (EditorGUI.EndChangeCheck())
            //{
            //    line.Point1 = lineTransform.InverseTransformPoint(point1);
            //}
            //EditorGUI.BeginChangeCheck();
            //Handles.DoPositionHandle(point2, lineRotation);
            //if (EditorGUI.EndChangeCheck())
            //{
            //    line.Point2 = lineTransform.InverseTransformPoint(point2);
            //}
        }
    }
}
