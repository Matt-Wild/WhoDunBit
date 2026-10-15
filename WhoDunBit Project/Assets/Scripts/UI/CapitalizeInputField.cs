using UnityEngine;

namespace WhoDunBit.UI
{
    [RequireComponent(typeof(TMPro.TMP_InputField))]
    public class CapitalizeInputField : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_InputField _inputField;

        private void Awake()
        {
            if (_inputField == null)
            {
                Debug.LogWarning("InputField reference is missing!", this);
            }

            _inputField.onValueChanged.AddListener(CapitalizeInput);
        }

        private void OnValidate()
        {
            if (_inputField == null)
            {
                _inputField = GetComponent<TMPro.TMP_InputField>();
            }
        }

        private void CapitalizeInput(string input)
        {
            if (_inputField == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(input))
            {
                return;
            }

            _inputField.text = input.ToUpper();
        }
    }
}
