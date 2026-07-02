using Cysharp.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KoiAI.Utilities
{
    public enum ComparisonType
    {
        None,
        LessEqual,
        GreaterEqual,
        Less,
        Greater,
        Equal
    }

    [Serializable]
    public struct CompareValueCondition<T> where T : IComparable<T>
    {
        [SerializeField]
        private T _compareValue;
        [SerializeField]
        private ComparisonType _comparisonType;

        public CompareValueCondition(T compareValue, ComparisonType comparisonType)
        {
            _compareValue = compareValue;
            _comparisonType = comparisonType;
        }

        public T CompareValue => _compareValue;
        public ComparisonType ComparisonType => _comparisonType;
    }

    [Serializable]
    public struct CompareEnumCondition<T> where T : Enum
    {
        [SerializeField]
        private T _compareValue;
        [SerializeField]
        private ComparisonType _comparisonType;
        public T CompareValue => _compareValue;
        public ComparisonType ComparisonType => _comparisonType;

    }

    public static class CompareCondition
    {
        /// <summary>
        /// 조건데이터를 통해 비교합니다.
        /// </summary>
        public static bool CompareWithCondition<T>(this T curValue, CompareValueCondition<T> compareValueCondition) where T : IComparable<T>
        {
            if(IsNoneCondition(compareValueCondition))
            {
                return false;
            }
            int compareResult = curValue.CompareTo(compareValueCondition.CompareValue);
            bool result = compareValueCondition.ComparisonType switch
            {
                ComparisonType.LessEqual => compareResult <= 0,
                ComparisonType.GreaterEqual => compareResult >= 0,
                ComparisonType.Less => compareResult < 0,
                ComparisonType.Greater => compareResult > 0,
                ComparisonType.Equal => compareResult == 0,
                _ => false
            };
            return result;
        }

        /// <summary>
        /// Enum 조건데이터를 통해 비교합니다.
        /// </summary>
        public static bool CompareEnumWithCondition<T, TCompare>(this T curValue, CompareEnumCondition<T> compareEnumCondition) where T : Enum where TCompare : IComparable<TCompare>
        {
            T enumCompareValue = compareEnumCondition.CompareValue;
            if(Enum.GetUnderlyingType(typeof(T)) == typeof(TCompare))
            {
                return false;
            }
            //변환을 하지않고 메모리 주소를 해당 (int, long...)타입으로 해석합니다.(따라서 박싱이 없습니다.)
            TCompare compareValue = Unsafe.As<T, TCompare>(ref enumCompareValue);
            CompareValueCondition<TCompare> newValueCondition = new(compareValue, compareEnumCondition.ComparisonType);
            if (IsNoneCondition(newValueCondition))
            {
                return false;
            }

            bool result =  compareValue.CompareWithCondition(newValueCondition);
            return result; 
        }

        private static bool IsNoneCondition<T>(CompareValueCondition<T> compareValueCondition) where T : IComparable<T> => compareValueCondition.ComparisonType == ComparisonType.None;
    }

}
