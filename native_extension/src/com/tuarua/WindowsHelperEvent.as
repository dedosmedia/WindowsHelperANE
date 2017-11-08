/**
 * Created by Eoin Landy on 02/05/2017.
 */
package com.tuarua {
import flash.events.Event;

public class WindowsHelperEvent extends Event {
    public var params:Object;
    public static const UPLOAD_COMPLETE:String = "UPLOAD_COMPLETE";

    public function WindowsHelperEvent(type:String, params:Object=null, bubbles:Boolean=false, cancelable:Boolean=false) {
        super(type, bubbles, cancelable);
        this.params = params;
    }
    public override function clone():Event {
        return new HotKeyEvent(type, this.params, bubbles, cancelable);
    }
    public override function toString():String {
        return formatToString("ToastEvent", "params", "type", "bubbles", "cancelable");
    }
}
}
