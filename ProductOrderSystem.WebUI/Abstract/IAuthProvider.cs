using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProductOrderSystem.WebUI.Abstract
{
    public interface IAuthProvider
    {
        bool Authenticate(string username, string password);
    }
}
