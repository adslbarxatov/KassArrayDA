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
		// Ресивер сообщений на повторное открытие окна
		private EventWaitHandle ewh;

		// Список сохранённых реквизитов
		private KADAMath km;

		/// <summary>
		/// Конструктор. Запускает главную форму
		/// </summary>
		/// <param name="FilePath">Путь к файлу для открытия из командной строки;
		/// может быть пустой строкой</param>
		public KassArrayDAForm (string FileName)
			{
			// Инициализация
			InitializeComponent ();
			RDGenerics.LoadWindowDimensions (this);

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
			DataTemplateCombo.Items.AddRange (KADAMath.DataTemplateNames);
			DataTemplateCombo.SelectedIndex = 0;

			ExportTemplateCombo.Items.AddRange (KADAMath.ExportTemplateNames);
			ExportTemplateCombo.SelectedIndex = 0;

			RDLocale.SetDefaultControlText (MAbout, RDLDefaultTexts.Control_AppAbout);
			RDLocale.SetDefaultControlText (MExit, RDLDefaultTexts.Button_Exit);
			RDLocale.SetDefaultControlText (MOpen, RDLDefaultTexts.Button_Open);
			RDLocale.SetDefaultControlText (MSave, RDLDefaultTexts.Button_Save);

			SFDialog.Title = "Экспорт данных";
			SFDialog.Filter = "Табличные данные (*.csv)|*.csv";
			
			KODialog.Title = "Загрузка ранее импортированных данных";
			KSDialog.Title = "Сохранение импортированных данных";
			KODialog.Filter = KSDialog.Filter = "Файлы данных, извлечённых из выгрузок ОФД (*" +
				KADAMath.DataFileExt + ")|*" + KADAMath.DataFileExt;
			KODialog.Filter += "|Файлы данных, загруженных из ФН (*" + FSDInterface.FileExtension +
				")|*" + FSDInterface.FileExtension;

			DocTypeCombo.Items.Add ("Продажи минус возвраты");
			DocTypeCombo.Items.Add ("Продажи и возвраты отдельно");
			DocTypeCombo.SelectedIndex = 0;

			// Попытка открытия указанного файла
			if (!string.IsNullOrWhiteSpace (FileName))
				{
				KODialog.FileName = FileName;
				MOpen_Click (null, null);
				}
			else
				{
				// Попытка загрузки автосохранения
				km = new KADAMath ();
				UpdateStatus ();
				}
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

			/*DocTypeCombo.Items.Clear ();
			DocTypeCombo.Items.AddRange (km.AvailableDocumentTypes);
			DocTypeCombo.SelectedIndex = 0;*/

			TaxCombo.Items.Clear ();
			TaxCombo.Items.AddRange (km.AvailableTaxSystems);
			TaxCombo.SelectedIndex = 0;

			SessionCombo.Items.Clear ();
			SessionCombo.Items.AddRange (km.AvailableSessionNumbers);
			SessionCombo.SelectedIndex = 0;
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
			string path = KassArrayDB::RD_AAOW.KKTSupport.PathForStartupOpening;
			if (string.IsNullOrWhiteSpace (path) && this.Visible &&
				(this.WindowState != FormWindowState.Minimized) || !ewh.WaitOne (100))
				{
				ewh.Reset ();	// Удаление задвоенных вызовов
				return;
				}

			// Отмена повторного обращения
			ewh.Reset ();

			// Запуск
			this.Show ();

			this.TopMost = true;
			this.TopMost = false;
			this.WindowState = FormWindowState.Normal;

			// Запуск файла, если он был передан
			if (!string.IsNullOrWhiteSpace (path))
				{
				KassArrayDB::RD_AAOW.KKTSupport.PathForStartupOpening = "";

				KODialog.FileName = path;
				MOpen_Click (null, null);
				}
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

			CheckRollBack ();
			}

		// Откат к предыдущему сохранению
		private void CheckRollBack ()
			{
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

			if (km.ExportData (SFDialog.FileName, (ExportTemplates)ExportTemplateCombo.SelectedIndex,
				DocTypeCombo.SelectedIndex == 1, (byte)TaxCombo.SelectedIndex, (uint)SessionCombo.SelectedIndex))
				RDInterface.MessageBox (RDMessageFlags.Success | RDMessageFlags.CenterText,
					"Экспорт выполнен успешно", 750);
			else
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText | RDMessageFlags.LockSmallSize,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.Message_SaveFailure_Fmt),
					Path.GetFileName (SFDialog.FileName)));
			}

		// Открытие файла внутреннего формата
		private void MOpen_Click (object sender, EventArgs e)
			{
			// Запрос имени файла
			if ((sender != null) && (KODialog.ShowDialog () != DialogResult.OK))
				return;

			if (km != null)
				km.Dispose ();
			km = new KADAMath (KODialog.FileName, KODialog.FileName.EndsWith (FSDInterface.FileExtension));
			UpdateStatus ();

			if (!km.HasErrors)
				RDInterface.MessageBox (RDMessageFlags.Success | RDMessageFlags.CenterText,
					"Файл успешно загружен", 750);
			else
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText | RDMessageFlags.LockSmallSize,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.Message_LoadFailure_Fmt),
					Path.GetFileName (KODialog.FileName)));

			CheckRollBack ();
			}

		// Сохранение файла внутреннего формата
		private void MSave_Click (object sender, EventArgs e)
			{
			// Запрос имени файла
			if (KSDialog.ShowDialog () != DialogResult.OK)
				return;

			if (km.SaveData (KSDialog.FileName))
				RDInterface.MessageBox (RDMessageFlags.Success | RDMessageFlags.CenterText,
					"Файл успешно сохранён", 750);
			else
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText | RDMessageFlags.LockSmallSize,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.Message_SaveFailure_Fmt),
					Path.GetFileName (KSDialog.FileName)));
			}
		}
	}
