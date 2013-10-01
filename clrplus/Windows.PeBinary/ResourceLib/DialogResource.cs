﻿//-----------------------------------------------------------------------
// <copyright company="CoApp Project">
//     ResourceLib Original Code from http://resourcelib.codeplex.com
//     Original Copyright (c) 2008-2009 Vestris Inc.
//     Changes Copyright (c) 2011 Garrett Serack . All rights reserved.
// </copyright>
// <license>
// MIT License
// You may freely use and distribute this software under the terms of the following license agreement.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of 
// the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE
// </license>
//-----------------------------------------------------------------------

namespace ClrPlus.Windows.PeBinary.ResourceLib {
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using Api.Enumerations;

    /// <summary>
    ///     A dialog template resource.
    /// </summary>
    public class DialogResource : Resource {
        private DialogTemplateBase _dlgtemplate;

        /// <summary>
        ///     A structured dialog resource embedded in an executable module.
        /// </summary>
        /// <param name="hModule">Module handle.</param>
        /// <param name="hResource">Resource handle.</param>
        /// <param name="type">Type of resource.</param>
        /// <param name="name">Resource name.</param>
        /// <param name="language">Language id.</param>
        /// <param name="size">Resource size.</param>
        public DialogResource(IntPtr hModule, IntPtr hResource, ResourceId type, ResourceId name, UInt16 language, int size)
            : base(hModule, hResource, type, name, language, size) {
        }

        /// <summary>
        ///     A structured dialog resource embedded in an executable module.
        /// </summary>
        public DialogResource()
            : base(IntPtr.Zero, IntPtr.Zero, new ResourceId(ResourceTypes.RT_DIALOG), new ResourceId(1), ResourceUtil.NEUTRALLANGID, 0) {
        }

        /// <summary>
        ///     A dialog template structure that describes the dialog.
        /// </summary>
        public DialogTemplateBase Template {
            get {
                return _dlgtemplate;
            }
            set {
                _dlgtemplate = value;
            }
        }

        internal override IntPtr Read(IntPtr hModule, IntPtr lpRes) {
            switch ((uint)Marshal.ReadInt32(lpRes) >> 16) {
                case 0xFFFF:
                    _dlgtemplate = new DialogExTemplate();
                    break;
                default:
                    _dlgtemplate = new DialogTemplate();
                    break;
            }

            // dialog structure itself
            return _dlgtemplate.Read(lpRes);
        }

        internal override void Write(BinaryWriter w) {
            _dlgtemplate.Write(w);
        }

        /// <summary>
        ///     Dialog resource in standard resource editor text format.
        /// </summary>
        /// <returns>Multi-line string.</returns>
        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} {1}", Name.IsIntResource() ? Name.ToString() : "\"" + Name + "\"", _dlgtemplate);
            return sb.ToString();
        }
    }
}