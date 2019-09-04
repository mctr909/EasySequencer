        public List<Event> Copy(uint beginTime, uint endTime, byte lowNote, byte highNote) {
            return getTargetList(beginTime, endTime, lowNote, highNote);
        }

        public List<Event> Cut(uint beginTime, uint endTime, byte lowNote, byte highNote) {
            var notTarget = new List<Event>();
            var target = getTargetList(beginTime, endTime, lowNote, highNote, notTarget);
            mEventList.Clear();
            mEventList.AddRange(notTarget);
            return target;
        }

        public List<Event> Past(List<Event> eventList) {
            var overlapList = getOverlapList(eventList);
            if (0 == overlapList.Count) {
                mEventList.AddRange(eventList);
                mEventList.Sort(Event.Compare);
            }
            return overlapList;
        }

        private List<Event> getOverlapList(List<Event> eventList) {
            var overlapList = new List<Event>();
            for (int c = 0; c < eventList.Count; ++c) {
                var checkEvent = eventList[c];
                var checkMsg = checkEvent.Message;
                var checkTime = checkEvent.Time;
                if (E_EVENT_TYPE.NOTE_ON != checkMsg.Type && E_EVENT_TYPE.NOTE_OFF != checkMsg.Type) {
                    continue;
                }

                for (int i = 0; i < mEventList.Count; ++i) {
                    var curMsg = mEventList[i].Message;
                    var curTime = mEventList[i].Time;
                    uint beginTime = 0xFFFFFFFF;
                    uint endTime = 0;
                    switch (curMsg.Type) {
                    case E_EVENT_TYPE.NOTE_ON:
                        if (checkMsg.Data[0] != curMsg.Data[0]) {
                            break;
                        }
                        beginTime = curTime;
                        for (int j = i; j < mEventList.Count; ++j) {
                            var nxMsg = mEventList[j].Message;
                            var nxTime = mEventList[j].Time;
                            if (E_EVENT_TYPE.NOTE_OFF == nxMsg.Type && curMsg.Data[0] == nxMsg.Data[0]) {
                                endTime = nxTime;
                                break;
                            }
                        }
                        break;
                    case E_EVENT_TYPE.NOTE_OFF:
                        if (checkMsg.Data[0] != curMsg.Data[0]) {
                            break;
                        }
                        endTime = curTime;
                        for (int j = i; 0 <= j; --j) {
                            var bkMsg = mEventList[j].Message;
                            var bkTime = mEventList[j].Time;
                            if (E_EVENT_TYPE.NOTE_ON == bkMsg.Type && curMsg.Data[0] == bkMsg.Data[0]) {
                                beginTime = bkTime;
                                break;
                            }
                        }
                        break;
                    }
                    if (E_EVENT_TYPE.NOTE_ON == checkMsg.Type) {
                        if (beginTime <= checkTime && checkTime < endTime) {
                            overlapList.Add(checkEvent);
                            break;
                        }
                    } else {
                        if (beginTime < checkTime && checkTime <= endTime) {
                            overlapList.Add(checkEvent);
                            break;
                        }
                    }
                }
            }
            return overlapList;
        }

        private List<Event> getTargetList(uint beginTime, uint endTime, byte lowNote, byte highNote, List<Event> notTargetList = null) {
            var targetList = new List<Event>();
            for (int i = 0; i < mEventList.Count; ++i) {
                var checkEv = mEventList[i];
                var checkMsg = checkEv.Message;
                var checkTime = checkEv.Time;
                var isTarget = false;
                if (beginTime <= checkTime && checkTime <= endTime) {
                    switch (checkMsg.Type) {
                    case E_EVENT_TYPE.NOTE_ON:
                        if (checkMsg.Data[0] < lowNote || highNote < checkMsg.Data[0]) {
                            break;
                        }
                        for (int j = i; j < mEventList.Count; ++j) {
                            var nxMsg = mEventList[j].Message;
                            var nxTime = mEventList[j].Time;
                            if (E_EVENT_TYPE.NOTE_OFF == nxMsg.Type && checkMsg.Data[0] == nxMsg.Data[0]) {
                                if (beginTime <= nxTime && nxTime <= endTime) {
                                    isTarget = true;
                                    break;
                                }
                            }
                        }
                        break;
                    case E_EVENT_TYPE.NOTE_OFF:
                        if (checkMsg.Data[0] < lowNote || highNote < checkMsg.Data[0]) {
                            break;
                        }
                        for (int j = i; 0 <= j; --j) {
                            var bkMsg = mEventList[j].Message;
                            var bkTime = mEventList[j].Time;
                            if (E_EVENT_TYPE.NOTE_ON == bkMsg.Type && checkMsg.Data[0] == bkMsg.Data[0]) {
                                if (beginTime <= bkTime && bkTime <= endTime) {
                                    isTarget = true;
                                    break;
                                }
                            }
                        }
                        break;
                    default:
                        isTarget = true;
                        break;
                    }
                }
                if (isTarget) {
                    targetList.Add(checkEv);
                } else {
                    if (null != notTargetList) {
                        notTargetList.Add(checkEv);
                    }
                }
            }
            return targetList;
        }