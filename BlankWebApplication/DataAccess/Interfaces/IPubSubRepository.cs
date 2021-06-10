using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JosephJulianMuscatMSD63ASynoptic.DataAccess.Interfaces
{
    public interface IPubSubRepository
    {
        void PublishMessage(string msg, string hasImage);

        string PullMessage();
    }
}