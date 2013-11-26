using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuierobesarteApp.Model
{
    public class AppModel
    {
        WeddingDto _currentWedding;

        public WeddingDto CurrentWedding
        {
            get { return _currentWedding; }
            set { _currentWedding = value; }
        }


        public string CurrentUserName { get; set; }
        public bool ShowUserPanel { get; set; }
    }
}
