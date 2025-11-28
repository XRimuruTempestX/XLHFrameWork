using XLHFramework.UIFrameWork.Agent; 
using XLHFramework.UIFrameWork.Runtime.Base;
using Cysharp.Threading.Tasks; 
using UnityEngine.UI;
using UnityEngine; 
using TMPro;

namespace UIFrameworlk
{
	public class RegisterWindowDataWindow : MonoBehaviour
	{
		public TMP_InputField userNameTMP_InputField;
		public TMP_InputField passWordTMP_InputField;
		public Button RegisterBtn;
		public Button CancelBtn;

		public void InitComponent(WindowBase target)
		{
			RegisterWindow mWindow = (RegisterWindow)target;

			userNameTMP_InputField.BindTMP_InputFieldValueChanged(mWindow.AdduserNameTMP_InputFieldValueChangedListener);
			userNameTMP_InputField.BindTMP_InputFieldEndEdit(mWindow.AdduserNameTMP_InputFieldEndEditListener);
			passWordTMP_InputField.BindTMP_InputFieldValueChanged(mWindow.AddpassWordTMP_InputFieldValueChangedListener);
			passWordTMP_InputField.BindTMP_InputFieldEndEdit(mWindow.AddpassWordTMP_InputFieldEndEditListener);
			RegisterBtn.BindButtonClick(mWindow.AddRegisterBtnListener);
			CancelBtn.BindButtonClick(mWindow.AddCancelBtnListener);
		}
	}
}
