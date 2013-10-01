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

    /// <summary>
    ///     An extended popup menu item.
    /// </summary>
    public class MenuExTemplateItemPopup : MenuExTemplateItem {
        private UInt32 _dwHelpId;
        private MenuExTemplateItemCollection _subMenuItems = new MenuExTemplateItemCollection();

        /// <summary>
        ///     Sub menu items.
        /// </summary>
        public MenuExTemplateItemCollection SubMenuItems {
            get {
                return _subMenuItems;
            }
            set {
                _subMenuItems = value;
            }
        }

        /// <summary>
        ///     Read an extended popup menu item.
        /// </summary>
        /// <param name="lpRes">Address in memory.</param>
        /// <returns>End of the menu item structure.</returns>
        internal override IntPtr Read(IntPtr lpRes) {
            lpRes = base.Read(lpRes);

            lpRes = ResourceUtil.Align(lpRes);
            _dwHelpId = (UInt32)Marshal.ReadInt32(lpRes);
            lpRes = new IntPtr(lpRes.ToInt32() + 4);

            return _subMenuItems.Read(lpRes);
        }

        /// <summary>
        ///     Write the menu item to a binary stream.
        /// </summary>
        /// <param name="w">Binary stream.</param>
        internal override void Write(BinaryWriter w) {
            base.Write(w);
            ResourceUtil.PadToDWORD(w);
            w.Write(_dwHelpId);
            _subMenuItems.Write(w);
        }

        /// <summary>
        ///     String representation in the MENUEX format.
        /// </summary>
        /// <returns>String representation.</returns>
        public override string ToString(int indent) {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0}POPUP \"{1}\"", new String(' ', indent), _menuString == null ? string.Empty : _menuString.Replace("\t", @"\t")));
            sb.Append(_subMenuItems.ToString(indent));
            return sb.ToString();
        }
    }
}