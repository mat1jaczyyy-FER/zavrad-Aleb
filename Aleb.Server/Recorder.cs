using System;
using System.Collections.Generic;
using System.Linq;

using Aleb.Common;

namespace Aleb.Server {
    class Recorder: Replayable {
        public readonly RecordedMatch Recording = new RecordedMatch();

        public Recorder(List<User> users)
        : base(users) => Recording.NewRound();

        protected override void BeforeRecordsWrite()
            => Recording.WriteRecords(TempRecords.ToList());

        protected override void BeforeRecordsCleared()
            => Recording.NewRound();
    }
}
