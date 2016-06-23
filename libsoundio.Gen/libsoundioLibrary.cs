using System;
using System.Linq;
using CppSharp;
using CppSharp.AST;
using CppSharp.Generators;
using CppSharp.Passes;
using CppSharp.Types;
using CppSharp.Utils;

namespace libsoundio.Gen
{
	internal class libsoundioLibrary : ILibrary
	{
		public void Postprocess(Driver driver, ASTContext ctx)
		{
			//throw new NotImplementedException();
			//ctx.enum
			//SoundIoChannelLayout  Internal SoundIoChannelId
			
			//ctx.IgnoreClassField("SoundIoChannelArea", "ptr");	

			var soundio = driver.ASTContext.TranslationUnits.FirstOrDefault(t => t.FileName == "soundio.h");
			if (soundio == null)
				return;


			
			/*/var SoundIoChannelLayout = soundio.FindClass("SoundIoChannelLayout");

			var channels = SoundIoChannelLayout.Fields.FirstOrDefault(t => t.InternalName == "channels");*/

			//annels.
			//string s = Marshal.PtrToStringAnsi((IntPtr)c);
		}

		public void Preprocess(Driver driver, ASTContext ctx)
		{
			var soundio = driver.ASTContext.TranslationUnits.FirstOrDefault(t => t.FileName == "soundio.h");
			if (soundio == null)
				return;

			foreach (var c in soundio.Classes)
			{
				//foreach (var f in c.Fields)
					//f.ExplicityIgnored = true;
				//c.IsOpaque = true;
			}

			/*{
				var SoundIoChannelLayout = soundio.FindClass("SoundIoChannelLayout");
				var channels = SoundIoChannelLayout.Fields.FirstOrDefault(t => t.Name == "channels");

				var at = channels.Type as ArrayType;
				if (at != null)
					at.QualifiedType = new QualifiedType(new BuiltinType(PrimitiveType.Int), at.QualifiedType.Qualifiers);
			}
			{
				var SoundIoChannelArea = soundio.FindClass("SoundIoChannelArea");
			}*/
		}

		public void Setup(Driver driver)
		{
			var options = driver.Options;
			options.GeneratorKind = GeneratorKind.CSharp;
			options.LanguageVersion = CppSharp.Parser.LanguageVersion.C;
			options.LibraryName = "libsoundio.NET";
			options.Headers.Add(@"F:\Libraries\libsoundio\soundio\soundio.h");
			options.Libraries.Add(@"F:\Libraries\libsoundio\build64\Debug\soundio.lib");
			options.OutputDir = @"F:\Projects\libsoundio.NET";
		}

		public void SetupPasses(Driver driver)
		{
			//driver.TranslationUnitPasses.AddPass(new TypeParameterRenamePass("SoundIoChannelId",
            //    new BuiltinType(PrimitiveType.Int), RenameTargets.Field));
			//throw new NotImplementedException();
		}
	}

	/*internal class TypeParameterRenamePass : RenamePass
	{
		private readonly CppSharp.AST.Type _type;
		private readonly string _typeName;

		public TypeParameterRenamePass(string typeName, CppSharp.AST.Type type, RenameTargets targets)
		{
			_typeName = typeName;
			_type = type;
			Targets = targets;
		}

		public override bool Rename(Declaration decl, out string newName)
		{
			decl.
			if (decl.Name == _typeName)
			{
				var p = decl as Parameter;
				if (p != null)
				{
					p.QualifiedType = new QualifiedType(_type, p.QualifiedType.Qualifiers);
					newName = _typeName;
					return true;
				}
			}
			newName = null;
			return false;
		}
	}*/
}