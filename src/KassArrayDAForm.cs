extern alias KassArrayDB;

using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает главную форму программы
	/// </summary>
	public partial class KassArrayDAForm: Form
		{
		/*// Переменные и константы
		private KassArrayDB::RD_AAOW.KnowledgeBase kb;*/

		// Ресивер сообщений на повторное открытие окна
		private EventWaitHandle ewh;

		// Список сохранённых реквизитов
		private KADAMath km;

		/// <summary>
		/// Конструктор. Запускает главную форму
		/// </summary>
		public KassArrayDAForm ()
			{
			// Инициализация
			InitializeComponent ();
			RDGenerics.LoadWindowDimensions (this);

			/*kb = new KassArrayDB::RD_AAOW.KnowledgeBase ();*/
			this.Text = RDGenerics.DefaultAssemblyVisibleName;

			// Подключение к прослушиванию системного события вызова окна
			if (!RDGenerics.StartedFromMSStore)
				{
				bool ewhFailed = false;
				try
					{
					ewh = EventWaitHandle.OpenExisting (ProgramDescription.AssemblyMainName);
					}
				catch
					{
					ewhFailed = true;
					}
				if (ewhFailed)
					{
					try
						{
						ewh = new EventWaitHandle (false, EventResetMode.AutoReset,
							ProgramDescription.AssemblyMainName);
						}
					catch { }
					}

				ShowWindowTimer.Enabled = true;
				}

			// Настройка контролов
			/*// !!! ТЕСТ
			km = new KADAMath ("C:\\Users\\Бархатов Николай\\Desktop\\Первый ОФД", DataTemplates.ESKCompleteExport);
			km.ExportUsableData ("TestExp.csv");*/
			DataTemplateCombo.Items.AddRange (KADAMath.DataTemplateNames);
			DataTemplateCombo.SelectedIndex = 0;

			ExportTemplateCombo.Items.AddRange (KADAMath.ExportTemplateNames);
			ExportTemplateCombo.SelectedIndex = 0;

			RDLocale.SetDefaultControlText (MAbout, RDLDefaultTexts.Control_AppAbout);
			RDLocale.SetDefaultControlText (MExit, RDLDefaultTexts.Button_Exit);

			SFDialog.Title = "Экспорт данных";
			SFDialog.Filter = "Табличные данные (*.csv)|*.csv";

			// Попытка загрузки автосохранения
			km = new KADAMath ();
			UpdateStatus ();
			/*km.ExportUsableData ("TextExp.csv");*/
			}

		// Обновление статуса
		private void UpdateStatus ()
			{
			if (km.DocumentsCount > 0)
				FastResultLabel.Text = "Загружено: " + km.DocumentsCount.ToString ("#,0") + " ФД" + RDLocale.RN +
					"с " + km.MinimumDate.ToString (KADAMath.DateTimeFormat) +
					" по " + km.MaximumDate.ToString (KADAMath.DateTimeFormat);
			else
				FastResultLabel.Text = "(импортируйте данные)";
			}

		// Отображение справки
		private void MHelp_Clicked (object sender, EventArgs e)
			{
			RDInterface.ShowAbout (false);
			}

		// Таймер обратной связи с вызывающим приложением
		private void ShowWindowTimer_Tick (object sender, EventArgs e)
			{
			// Контроль
			if (ewh == null)
				return;

			// Защита от лишних действий
			if (this.Visible && (this.WindowState != FormWindowState.Minimized) || !ewh.WaitOne (100))
				{
				ewh.Reset ();   // Удаление задвоенных вызовов
				return;
				}

			// Отмена повторного обращения
			ewh.Reset ();

			// Запуск
			this.Show ();

			this.TopMost = true;
			this.TopMost = false;
			this.WindowState = FormWindowState.Normal;
			}

		// Загрузка
		private void BLoad_Click (object sender, EventArgs e)
			{
			// Подготовка
			if (FBDialog.ShowDialog () != DialogResult.OK)
				return;

			// Запуск
			km.Dispose ();
			km = new KADAMath (FBDialog.SelectedPath, (DataTemplates)DataTemplateCombo.SelectedIndex);
			UpdateStatus ();

			// Откат
			if (km.HasErrors && (RDInterface.MessageBox (RDMessageFlags.Question | RDMessageFlags.CenterText,
				"Восстановить данные предыдущей загрузки из резервной копии?",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Yes),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)) == RDMessageButtons.ButtonOne))
				{
				km.Dispose ();
				km = new KADAMath ();
				UpdateStatus ();
				}
			}

		// Выход из приложения
		private void MExit_Click (object sender, EventArgs e)
			{
			this.Close ();
			}

		private void KassArrayDAForm_FormClosing (object sender, FormClosingEventArgs e)
			{
			RDGenerics.SaveWindowDimensions (this);
			}

		// Экспорт данных
		private void BExport_Click (object sender, EventArgs e)
			{
			// Запрос имени файла
			SFDialog.FileName = ExportTemplateCombo.Text;
			if (SFDialog.ShowDialog () != DialogResult.OK)
				return;

			if (km.ExportData (SFDialog.FileName, (ExportTemplates)ExportTemplateCombo.SelectedIndex))
				RDInterface.MessageBox (RDMessageFlags.Success | RDMessageFlags.CenterText,
					"Экспорт выполнен успешно", 750);
			else
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText | RDMessageFlags.LockSmallSize,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.Message_SaveFailure_Fmt),
					Path.GetFileName (SFDialog.FileName)));
			}
		}
	}
