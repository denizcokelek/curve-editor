using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project
{
    public class CurveDecorator : MonoBehaviour
    {
        #region Private Fields

        [SerializeField] private BezierCurve _bezierCurve;
        [SerializeField] private GameObject _gameObject;
        [SerializeField] public int _frequency;

        #endregion
        
        private void Awake()
        {
            for (int i = 0; i <= _frequency; i++)
            {
                Vector3 nextPoint = _bezierCurve.GetPointAtCurve(i / (float)_frequency);
                Quaternion rotation = Quaternion.LookRotation(_bezierCurve.GetDirection(i / (float) _frequency));
                Instantiate(_gameObject, nextPoint, rotation, transform);
            }
        }

    }
}
