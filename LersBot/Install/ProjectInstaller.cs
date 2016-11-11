using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace LersBot
{
	[RunInstaller(true)]
	public partial class ProjectInstaller : Installer
	{
		public ProjectInstaller()
		{
			InitializeComponent();
		}

		protected override void OnBeforeInstall(IDictionary savedState)
		{
			Context.Parameters["assemblypath"] = $@"""{Context.Parameters["assemblypath"]}"" /service";

			base.OnBeforeInstall(savedState);
		}
	}
}
