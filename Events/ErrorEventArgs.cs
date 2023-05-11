//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;

namespace PhlozLib
{
    public class ErrorEventArgs : EventArgs
    {
        public string ErrorMessage = "";

        public ErrorEventArgs(string Msg)
        {
            ErrorMessage = Msg;
        }
    }
}
