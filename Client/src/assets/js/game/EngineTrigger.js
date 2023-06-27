(function (global, undefined) {
  "use strict";

  var document = global.document
  var EngineTrigger;
  var EngineEvent = {
    create: function (eventName) {
      return {
        eventName: eventName,
        callbacks: [],
        registerCallback: function (callback) {
          this.callbacks.push(callback);
        },
        unregisterCallback: function (callback) {
          var callbackIndex = this.callbacks.indexOf(callback);

          if (callbackIndex > -1) {
            this.callbacks.splice(callbackIndex, 1);
          }
        },
        fire: function (data) {
          var callbacks = this.callbacks.slice(0);
          callbacks.forEach(function (callback) { callback(data); });
        }
      };
    },
  };

  EngineTrigger = function () {
    var _EngineTrigger = {};
    _EngineTrigger = {
      events: {},
      dispatch: function (eventName, data) {
        var event = this.events[eventName];
        if (event) {
          event.fire(data);
        }
      },
      on: function (eventName, callback) {
        var event = this.events[eventName];
        if (!event) {
          event = EngineEvent.create(eventName);
          this.events[eventName] = event;
        }

        event.registerCallback(callback);
      },
      off: function (eventName, callback) {
        var event = this.events[eventName];
        if (event) {
          event.unregisterCallback(callback);
          if (event.callbacks.length === 0) {
            delete this.events[eventName];
            //this.events[eventName] = null;
          }
        }
      }
    }
    return window.EngineTrigger = {
      events: _EngineTrigger.events,
      dispatch: function (eventName, data) {
        return _EngineTrigger.dispatch(eventName, data);
      },
      on: function (eventName, callback) {
        return _EngineTrigger.on(eventName, callback);
      },
      off: function (eventName, callback) {
        return _EngineTrigger.off(eventName, callback);
      }
    };
  };

  // AMD and window support
  if (typeof define === "function") {
    define([], function () { return new EngineTrigger(); });
  } else if (typeof global.EngineTrigger === "undefined") {
    global.EngineTrigger = new EngineTrigger();
  }
}(window));
