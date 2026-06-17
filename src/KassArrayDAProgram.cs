extern alias KassArrayDB;

using System;
using System.Threading;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает точку входа приложения
	/// </summary>
	public static class KADAProgram
		{
		/// <summary>
		/// Главная точка входа для приложения
		/// </summary>
		[STAThread]
		public static void Main (string[] args)
			{
			// Инициализация
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);
			RDLocale.InitEncodings ();

			// Язык интерфейса и контроль XPUN
			if (!RDLocale.CheckXPUNClass ())
				return;

			// Проверка запуска единственной копии (псевдоним не должен совпадать с именем EventWaitHandle)
			if (!RDGenerics.IsAppInstanceUnique (false, "_L"))
				{
				if (args.Length > 0)
					KassArrayDB::RD_AAOW.KKTSupport.PathForStartupOpening = args[0];

				try
					{
					EventWaitHandle ewh = EventWaitHandle.OpenExisting (ProgramDescription.AssemblyMainName);
					ewh.Set ();
					}
				catch { }

				return;
				}

			// Контроль прав и целостности
			if (!RDGenerics.AppHasAccessRights (true, false))
				return;

			if (!RDGenerics.StartedFromMSStore)
				{
				if (!RDGenerics.CheckLibrariesExistence (ProgramDescription.AssemblyLibraries, true))
					return;

				if (!LibrarySupport.CheckProtocolVersion (ProgramDescription.AssemblyDLLProtocol,
					KassArrayDB::RD_AAOW.ProgramDescription.KassArrayDBDLL))
					return;
				}

			// Отображение справки и запроса на принятие Политики
			if (!RDInterface.AcceptEULA ())
				return;
			if (!RDInterface.ShowAbout (true))
				RDGenerics.RegisterFileAssociations (true);

			// Запуск
			if (args.Length > 0)
				Application.Run (new KassArrayDAForm (args[0]));
			else
				Application.Run (new KassArrayDAForm (""));
			}
		}
	}
