using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMXTranslatorConsole
{
    class TranslationException : ApplicationException
    {
        public TranslationException()
            : base(AppResources.MSG_ERR_TRANSLATION)
        {
        }

        public TranslationException(string message)
            : base(message)
        {
        }

        public TranslationException(string message, Exception innerExp)
            : base(message, innerExp)
        {
        }

    }
}
