'use strict';
// JavaScript source code
(function() {
	var _global = this;
	var queuedEvents = [];
  var propertyId;
  var propertyKey;
  var interval = 3000;
  //var collectionEndPoint = 'https://localhost:44302/v1/event';
  var collectionEndPoint = 'https://collection.microserviceanalytics.com/v1/event';
  var timerId = null;
  var correlationIdPrefix = '';
  var autoStartJourneys = true;
  var currentJourney = null;
  var currentJourneyEndsOnError = true;
  var currentJourneyCreatedScope = false;
  var scopeCorrelationId = null;
  var httpWhitelist = [];
  var httpBlacklist = [];

  function scheduleNextUpload() {
    timerId = window.setTimeout(uploadData, interval);
  }

  function uploadData() {
    if (queuedEvents.length == 0) {
      scheduleNextUpload();
      return;
    }
    timerId = null;
    var payload = JSON.stringify({
      ApplicationVersion: "1.0.0.0",
      Source: "javascript",
      Events: queuedEvents
    });
    // do we want to build in some sense of retry on failure
    queuedEvents = [];
    var http = new XMLHttpRequest();
    http.onreadystatechange = function() {
      if (http.readyState == XMLHttpRequest.DONE && timerId == null) {
        scheduleNextUpload();
      }
    };
    http.open('POST', collectionEndPoint, true);
    http.setRequestHeader('af-property-id', propertyId);
    http.setRequestHeader('af-collection-key', propertyKey);
    http.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');
    http.send(payload);
  }

  function captureEvents() {
    document.body.addEventListener("click", clickBeginAutoJourney, true); // start on capture
    document.body.addEventListener("click", clickEndAutoJourney, false); // end on bubble back through
  }

  function clickBeginAutoJourney(ev) {
    var src = ev.srcElement;
    var journeyCode = src.getAttribute('data-journey-start');
    if (journeyCode) {
      analytics.startNamedJourney(journeyCode);
    }
  }
  function clickEndAutoJourney(ev) {
    if (currentJourney) {
      var src = ev.srcElement;
      var journeyCode = src.getAttribute('data-journey-start');
      var noStop = src.getAttribute('data-journey-noend')
      if (noStop === null && journeyCode && currentJourney.Data.JourneyCode == journeyCode) {
        analytics.endCurrentJourney(journeyCode);
      }
    }
  }
  function errorHandler(ev) {
    var evt = ev;
  }

  var analytics = {
    endJourneyAfterHttpRequest: false,
    journeyCodeOwnedByScope: false
  };
  function createCorrelationId() {
    if (scopeCorrelationId) {
      return scopeCorrelationId;
    }
    var correlationId = correlationIdPrefix + uuid.v4();
    if (currentJourney) {
      currentJourney.CorrelationIds.push(correlationId);
    }
    return correlationId;
  }
  function isHttpError(status) {
    return status !== 200;
  }
  // HTTP intercepts
	(function (open) {
	    XMLHttpRequest.prototype.open = function () {
        var that = this;
        var correlationId= createCorrelationId();
        this.onreadystatechange = function() {
          if (that.readyState === XMLHttpRequest.DONE && isHttpError(that.status)) {
            analytics.handleHttpError(that.status, that.response, correlationId);
          }
        };
        open.apply(this, arguments);
        this.setRequestHeader('correlation-id', correlationId);
	    };
	})(XMLHttpRequest.prototype.open);
	(function (send) {
	    XMLHttpRequest.prototype.send = function () {
	    	send.apply(this, arguments);
	    };
	})(XMLHttpRequest.prototype.send);	

  // Options include:
  //   propertyId - required, must match a property ID configured in the portal
  //   propertyKey - required, access key, must match a data access key for the property configured in the portal
  //   interval - optional, defaults to 3000, interval between sending batched client events to the analytic servers
  //   collectionEndPoint - optional, defaults to production servers, url to send events t
  //   correlationIdPrefix - optional, defaults to the property ID, a string to prefix correlation IDs with
  //   autoStartJourneys - optional, defaults to true, when enabled the events listed below trigger the start of a new journey when the source element as an attribute of data-journey. That attribute must name the journey.
  //   httpBlacklist - array of regex's that when matched will exclude those http calls from tracking
  //   httpWhitelist - array of regex's that when matched will include only those http calls in tracking
  // 
  // You can set both a whitelist and a blacklist but you only need one.
  //
  // Supported events for auto started journeys are:
  //   click
  analytics.configure = function(options) {
    propertyId = options.propertyId;
    propertyKey = options.propertyKey;
    if (options.endpoint) {
      collectionEndPoint = options.endpoint;
    }
    if (options.interval) {
      interval = options.interval;
    }
    if (options.collectionEndPoint) {
      collectionEndPoint = options.collectionEndPoint;
    }
    if (options.correlationIdPrefix) {
      correlationIdPrefix = options.correlationIdPrefix;
    }
    else {
      correlationIdPrefix = propertyId + '-';
    }
    if (options.autoStartJourneys) {
      autoStartJourneys = options.autoStartJourneys;
    }
    if (options.httpBlacklist) {
      httpBlacklist = options.httpBlacklist;
    }
    if (options.httpWhitelist) {
      httpWhitelist = options.httpWhitelist;
    }
    if (autoStartJourneys) {
      captureEvents();
    }
    window.addEventListener("error", errorHandler, true);
    window.onerror = errorHandler;
    scheduleNextUpload();
  };
  analytics.beginScope = function() {
    analytics.scopeCorrelationId = correlationIdPrefix + uuid.v4();
  };
  analytics.endScope = function() {
    analytics.scopeCorrelationId = null;
  };

  // shouldBeginsScope defaults to false, endsOnError defaults to true
	analytics.startJourney = function(code, shouldBeginScope, endsOnError) {
    // the scope and journey stuff is wrong. what we want to happen is:
    // 1. user begins journey (auto or manual)
    // 2. journey is marked as begun
    // 3. we collect all correlation-ids that are generated during the journey
    // 4. on journey end we stop collecting the correlation IDs and the journey is queued
    if (endsOnError === undefined) {
      endsOnError = true;
    }
    if (shouldBeginScope === undefined) {
      shouldBeginScope = false;
    }
    currentJourneyCreatedScope = false;
    if (shouldBeginScope && analytics.scopeCorrelationId === null) {
      analytics.beginScope();
      currentJourneyCreatedScope = true;
    }

    currentJourneyEndsOnError = endsOnError;
		currentJourney = {
      EventType: 'journey',
      EventStartDateTime: new Date().toISOString(),
      EventEndDateTime: null,
      CorrelationIds: [],
      Data: {
        JourneyCode: code,
        EndedWithError: false
      }
    };

    if (analytics.scopeCorrelationId) {
      currentJourney.CorrelationIds.push(analytics.scopeCorrelationId);
    }
	};
	analytics.endJourney = function(endedWithError) {
    if (currentJourneyCreatedScope) {
      analytics.endScope();
    }

		if (currentJourney) {
      currentJourney.EventEndDateTime = new Date().toISOString();
      if (endedWithError === true) {
        currentJourney.Data.EndedWithError = true;
      }
      queuedEvents.push(currentJourney);
      currentJourney = null;
    }
	};
  analytics.handleJavaScriptError = function(exception, cause) {
    if (currentJourney && currentJourneyEndsOnError) {
      analytics.endJourney(true);
    }
    var stackFrames = [];
    var parsedStackFrames = ErrorStackParser.parse(exception);
    for (var stackIndex= 0; stackIndex < parsedStackFrames.length; stackIndex++) {
      stackFrames.push({
        Filename: parsedStackFrames[stackIndex].fileName,
        Line: parsedStackFrames[stackIndex].lineNumber,
        Column: parsedStackFrames[stackIndex].columnNumber,
        Assembly: null,
        Class: null,
        Method: null
      });
    }
    var errorEvent = {
      EventType: 'error',
      EventStartDateTime: new Date().toISOString(),
      EventEndDateTime: null,
      CorrelationIds: [],
      Data: {
        StackFrames: stackFrames,
        Message: exception.message,
        ExceptionType: exception.name ? exception.name : 'javascript'
      }
    };

    queuedEvents.push(errorEvent);
  };
  analytics.handleHttpError = function(code, response, correlationId) {
    if (currentJourney && currentJourneyEndsOnError) {
      analytics.endJourney(true);
    }
  };

	_global.analytics = analytics;
}).call(this);


// AngularJS support
(function() {
  var _global = this;
  if (_global.angular) { 
    var errorHandlingModule = _global.angular.module('accidentalfish.errorhandling', []);
    errorHandlingModule.provider("$exceptionHandler", {
      $get: function(accidentalFishExceptionLoggingService) {
        return accidentalFishExceptionLoggingService;
      }
    });
    errorHandlingModule.factory("accidentalFishExceptionLoggingService", ['$log', '$window', function($log, $window) {
      function error(exception, cause) {
        $log.error.apply($log, arguments);
        analytics.handleJavaScriptError(exception, cause);
      }

      return error;
    }]);
  }
}).call(this);

//     uuid.js
//
//     Copyright (c) 2010-2012 Robert Kieffer
//     MIT License - http://opensource.org/licenses/mit-license.php

(function() {
  var _global = this;

  // Unique ID creation requires a high quality random # generator.  We feature
  // detect to determine the best RNG source, normalizing to a function that
  // returns 128-bits of randomness, since that's what's usually required
  var _rng;

  // Allow for MSIE11 msCrypto
  var _crypto = _global.crypto || _global.msCrypto;

  // Node.js crypto-based RNG - http://nodejs.org/docs/v0.6.2/api/crypto.html
  //
  // Moderately fast, high quality
  if (typeof(_global.require) == 'function') {
    try {
      var _rb = _global.require('crypto').randomBytes;
      _rng = _rb && function() {return _rb(16);};
    } catch(e) {}
  }

  if (!_rng && _crypto && _crypto.getRandomValues) {
    // WHATWG crypto-based RNG - http://wiki.whatwg.org/wiki/Crypto
    //
    // Moderately fast, high quality
    var _rnds8 = new Uint8Array(16);
    _rng = function whatwgRNG() {
      _crypto.getRandomValues(_rnds8);
      return _rnds8;
    };
  }

  if (!_rng) {
    // Math.random()-based (RNG)
    //
    // If all else fails, use Math.random().  It's fast, but is of unspecified
    // quality.
    var  _rnds = new Array(16);
    _rng = function() {
      for (var i = 0, r; i < 16; i++) {
        if ((i & 0x03) === 0) r = Math.random() * 0x100000000;
        _rnds[i] = r >>> ((i & 0x03) << 3) & 0xff;
      }

      return _rnds;
    };
  }

  // Buffer class to use
  var BufferClass = typeof(_global.Buffer) == 'function' ? _global.Buffer : Array;

  // Maps for number <-> hex string conversion
  var _byteToHex = [];
  var _hexToByte = {};
  for (var i = 0; i < 256; i++) {
    _byteToHex[i] = (i + 0x100).toString(16).substr(1);
    _hexToByte[_byteToHex[i]] = i;
  }

  // **`parse()` - Parse a UUID into it's component bytes**
  function parse(s, buf, offset) {
    var i = (buf && offset) || 0, ii = 0;

    buf = buf || [];
    s.toLowerCase().replace(/[0-9a-f]{2}/g, function(oct) {
      if (ii < 16) { // Don't overflow!
        buf[i + ii++] = _hexToByte[oct];
      }
    });

    // Zero out remaining bytes if string was short
    while (ii < 16) {
      buf[i + ii++] = 0;
    }

    return buf;
  }

  // **`unparse()` - Convert UUID byte array (ala parse()) into a string**
  function unparse(buf, offset) {
    var i = offset || 0, bth = _byteToHex;
    return  bth[buf[i++]] + bth[buf[i++]] +
            bth[buf[i++]] + bth[buf[i++]] + '-' +
            bth[buf[i++]] + bth[buf[i++]] + '-' +
            bth[buf[i++]] + bth[buf[i++]] + '-' +
            bth[buf[i++]] + bth[buf[i++]] + '-' +
            bth[buf[i++]] + bth[buf[i++]] +
            bth[buf[i++]] + bth[buf[i++]] +
            bth[buf[i++]] + bth[buf[i++]];
  }

  // **`v1()` - Generate time-based UUID**
  //
  // Inspired by https://github.com/LiosK/UUID.js
  // and http://docs.python.org/library/uuid.html

  // random #'s we need to init node and clockseq
  var _seedBytes = _rng();

  // Per 4.5, create and 48-bit node id, (47 random bits + multicast bit = 1)
  var _nodeId = [
    _seedBytes[0] | 0x01,
    _seedBytes[1], _seedBytes[2], _seedBytes[3], _seedBytes[4], _seedBytes[5]
  ];

  // Per 4.2.2, randomize (14 bit) clockseq
  var _clockseq = (_seedBytes[6] << 8 | _seedBytes[7]) & 0x3fff;

  // Previous uuid creation time
  var _lastMSecs = 0, _lastNSecs = 0;

  // See https://github.com/broofa/node-uuid for API details
  function v1(options, buf, offset) {
    var i = buf && offset || 0;
    var b = buf || [];

    options = options || {};

    var clockseq = options.clockseq != null ? options.clockseq : _clockseq;

    // UUID timestamps are 100 nano-second units since the Gregorian epoch,
    // (1582-10-15 00:00).  JSNumbers aren't precise enough for this, so
    // time is handled internally as 'msecs' (integer milliseconds) and 'nsecs'
    // (100-nanoseconds offset from msecs) since unix epoch, 1970-01-01 00:00.
    var msecs = options.msecs != null ? options.msecs : new Date().getTime();

    // Per 4.2.1.2, use count of uuid's generated during the current clock
    // cycle to simulate higher resolution clock
    var nsecs = options.nsecs != null ? options.nsecs : _lastNSecs + 1;

    // Time since last uuid creation (in msecs)
    var dt = (msecs - _lastMSecs) + (nsecs - _lastNSecs)/10000;

    // Per 4.2.1.2, Bump clockseq on clock regression
    if (dt < 0 && options.clockseq == null) {
      clockseq = clockseq + 1 & 0x3fff;
    }

    // Reset nsecs if clock regresses (new clockseq) or we've moved onto a new
    // time interval
    if ((dt < 0 || msecs > _lastMSecs) && options.nsecs == null) {
      nsecs = 0;
    }

    // Per 4.2.1.2 Throw error if too many uuids are requested
    if (nsecs >= 10000) {
      throw new Error('uuid.v1(): Can\'t create more than 10M uuids/sec');
    }

    _lastMSecs = msecs;
    _lastNSecs = nsecs;
    _clockseq = clockseq;

    // Per 4.1.4 - Convert from unix epoch to Gregorian epoch
    msecs += 12219292800000;

    // `time_low`
    var tl = ((msecs & 0xfffffff) * 10000 + nsecs) % 0x100000000;
    b[i++] = tl >>> 24 & 0xff;
    b[i++] = tl >>> 16 & 0xff;
    b[i++] = tl >>> 8 & 0xff;
    b[i++] = tl & 0xff;

    // `time_mid`
    var tmh = (msecs / 0x100000000 * 10000) & 0xfffffff;
    b[i++] = tmh >>> 8 & 0xff;
    b[i++] = tmh & 0xff;

    // `time_high_and_version`
    b[i++] = tmh >>> 24 & 0xf | 0x10; // include version
    b[i++] = tmh >>> 16 & 0xff;

    // `clock_seq_hi_and_reserved` (Per 4.2.2 - include variant)
    b[i++] = clockseq >>> 8 | 0x80;

    // `clock_seq_low`
    b[i++] = clockseq & 0xff;

    // `node`
    var node = options.node || _nodeId;
    for (var n = 0; n < 6; n++) {
      b[i + n] = node[n];
    }

    return buf ? buf : unparse(b);
  }

  // **`v4()` - Generate random UUID**

  // See https://github.com/broofa/node-uuid for API details
  function v4(options, buf, offset) {
    // Deprecated - 'format' argument, as supported in v1.2
    var i = buf && offset || 0;

    if (typeof(options) == 'string') {
      buf = options == 'binary' ? new BufferClass(16) : null;
      options = null;
    }
    options = options || {};

    var rnds = options.random || (options.rng || _rng)();

    // Per 4.4, set bits for version and `clock_seq_hi_and_reserved`
    rnds[6] = (rnds[6] & 0x0f) | 0x40;
    rnds[8] = (rnds[8] & 0x3f) | 0x80;

    // Copy bytes to buffer, if provided
    if (buf) {
      for (var ii = 0; ii < 16; ii++) {
        buf[i + ii] = rnds[ii];
      }
    }

    return buf || unparse(rnds);
  }

  // Export public API
  var uuid = v4;
  uuid.v1 = v1;
  uuid.v4 = v4;
  uuid.parse = parse;
  uuid.unparse = unparse;
  uuid.BufferClass = BufferClass;

  if (typeof(module) != 'undefined' && module.exports) {
    // Publish as node.js module
    module.exports = uuid;
  } else  if (typeof define === 'function' && define.amd) {
    // Publish as AMD module
    define(function() {return uuid;});
 

  } else {
    // Publish as global (in browsers)
    var _previousRoot = _global.uuid;

    // **`noConflict()` - (browser only) to reset global 'uuid' var**
    uuid.noConflict = function() {
      _global.uuid = _previousRoot;
      return uuid;
    };

    _global.uuid = uuid;
  }
}).call(this);

// StackFrame
// https://github.com/stacktracejs/stackframe
(function (root, factory) {
    'use strict';
    // Universal Module Definition (UMD) to support AMD, CommonJS/Node.js, Rhino, and browsers.

    /* istanbul ignore next */
    if (typeof define === 'function' && define.amd) {
        define('stackframe', [], factory);
    } else if (typeof exports === 'object') {
        module.exports = factory();
    } else {
        root.StackFrame = factory();
    }
}(this, function () {
    'use strict';
    function _isNumber(n) {
        return !isNaN(parseFloat(n)) && isFinite(n);
    }

    function StackFrame(functionName, args, fileName, lineNumber, columnNumber, source) {
        if (functionName !== undefined) {
            this.setFunctionName(functionName);
        }
        if (args !== undefined) {
            this.setArgs(args);
        }
        if (fileName !== undefined) {
            this.setFileName(fileName);
        }
        if (lineNumber !== undefined) {
            this.setLineNumber(lineNumber);
        }
        if (columnNumber !== undefined) {
            this.setColumnNumber(columnNumber);
        }
        if (source !== undefined) {
            this.setSource(source);
        }
    }

    StackFrame.prototype = {
        getFunctionName: function () {
            return this.functionName;
        },
        setFunctionName: function (v) {
            this.functionName = String(v);
        },

        getArgs: function () {
            return this.args;
        },
        setArgs: function (v) {
            if (Object.prototype.toString.call(v) !== '[object Array]') {
                throw new TypeError('Args must be an Array');
            }
            this.args = v;
        },

        // NOTE: Property name may be misleading as it includes the path,
        // but it somewhat mirrors V8's JavaScriptStackTraceApi
        // https://code.google.com/p/v8/wiki/JavaScriptStackTraceApi and Gecko's
        // http://mxr.mozilla.org/mozilla-central/source/xpcom/base/nsIException.idl#14
        getFileName: function () {
            return this.fileName;
        },
        setFileName: function (v) {
            this.fileName = String(v);
        },

        getLineNumber: function () {
            return this.lineNumber;
        },
        setLineNumber: function (v) {
            if (!_isNumber(v)) {
                throw new TypeError('Line Number must be a Number');
            }
            this.lineNumber = Number(v);
        },

        getColumnNumber: function () {
            return this.columnNumber;
        },
        setColumnNumber: function (v) {
            if (!_isNumber(v)) {
                throw new TypeError('Column Number must be a Number');
            }
            this.columnNumber = Number(v);
        },

        getSource: function () {
            return this.source;
        },
        setSource: function (v) {
            this.source = String(v);
        },

        toString: function() {
            var functionName = this.getFunctionName() || '{anonymous}';
            var args = '(' + (this.getArgs() || []).join(',') + ')';
            var fileName = this.getFileName() ? ('@' + this.getFileName()) : '';
            var lineNumber = _isNumber(this.getLineNumber()) ? (':' + this.getLineNumber()) : '';
            var columnNumber = _isNumber(this.getColumnNumber()) ? (':' + this.getColumnNumber()) : '';
            return functionName + args + fileName + lineNumber + columnNumber;
        }
    };

    return StackFrame;
}));

// StackTrace.parser
// https://github.com/stacktracejs/stacktrace.js/releases
(function (root, factory) {
    'use strict';
    // Universal Module Definition (UMD) to support AMD, CommonJS/Node.js, Rhino, and browsers.

    /* istanbul ignore next */
    if (typeof define === 'function' && define.amd) {
        define('error-stack-parser', ['stackframe'], factory);
    } else if (typeof exports === 'object') {
        module.exports = factory(require('stackframe'));
    } else {
        root.ErrorStackParser = factory(root.StackFrame);
    }
}(this, function ErrorStackParser(StackFrame) {
    'use strict';

    var FIREFOX_SAFARI_STACK_REGEXP = /(^|@)\S+\:\d+/;
    var CHROME_IE_STACK_REGEXP = /\s+at .*(\S+\:\d+|\(native\))/;

    return {
        /**
         * Given an Error object, extract the most information from it.
         * @param error {Error}
         * @return Array[StackFrame]
         */
        parse: function ErrorStackParser$$parse(error) {
            if (typeof error.stacktrace !== 'undefined' || typeof error['opera#sourceloc'] !== 'undefined') {
                return this.parseOpera(error);
            } else if (error.stack && error.stack.match(CHROME_IE_STACK_REGEXP)) {
                return this.parseV8OrIE(error);
            } else if (error.stack && error.stack.match(FIREFOX_SAFARI_STACK_REGEXP)) {
                return this.parseFFOrSafari(error);
            } else {
                throw new Error('Cannot parse given Error object');
            }
        },

        /**
         * Separate line and column numbers from a URL-like string.
         * @param urlLike String
         * @return Array[String]
         */
        extractLocation: function ErrorStackParser$$extractLocation(urlLike) {
            // Fail-fast but return locations like "(native)"
            if (urlLike.indexOf(':') === -1) {
                return [urlLike];
            }

            var locationParts = urlLike.replace(/[\(\)\s]/g, '').split(':');
            var lastNumber = locationParts.pop();
            var possibleNumber = locationParts[locationParts.length - 1];
            if (!isNaN(parseFloat(possibleNumber)) && isFinite(possibleNumber)) {
                var lineNumber = locationParts.pop();
                return [locationParts.join(':'), lineNumber, lastNumber];
            } else {
                return [locationParts.join(':'), lastNumber, undefined];
            }
        },

        parseV8OrIE: function ErrorStackParser$$parseV8OrIE(error) {
            return error.stack.split('\n').filter(function (line) {
                return !!line.match(CHROME_IE_STACK_REGEXP);
            }, this).map(function (line) {
                var tokens = line.replace(/^\s+/, '').split(/\s+/).slice(1);
                var locationParts = this.extractLocation(tokens.pop());
                var functionName = (!tokens[0] || tokens[0] === 'Anonymous') ? undefined : tokens[0];
                return new StackFrame(functionName, undefined, locationParts[0], locationParts[1], locationParts[2], line);
            }, this);
        },

        parseFFOrSafari: function ErrorStackParser$$parseFFOrSafari(error) {
            return error.stack.split('\n').filter(function (line) {
                return !!line.match(FIREFOX_SAFARI_STACK_REGEXP);
            }, this).map(function (line) {
                var tokens = line.split('@');
                var locationParts = this.extractLocation(tokens.pop());
                var functionName = tokens.shift() || undefined;
                return new StackFrame(functionName, undefined, locationParts[0], locationParts[1], locationParts[2], line);
            }, this);
        },

        parseOpera: function ErrorStackParser$$parseOpera(e) {
            if (!e.stacktrace || (e.message.indexOf('\n') > -1 &&
                e.message.split('\n').length > e.stacktrace.split('\n').length)) {
                return this.parseOpera9(e);
            } else if (!e.stack) {
                return this.parseOpera10(e);
            } else {
                return this.parseOpera11(e);
            }
        },

        parseOpera9: function ErrorStackParser$$parseOpera9(e) {
            var lineRE = /Line (\d+).*script (?:in )?(\S+)/i;
            var lines = e.message.split('\n');
            var result = [];

            for (var i = 2, len = lines.length; i < len; i += 2) {
                var match = lineRE.exec(lines[i]);
                if (match) {
                    result.push(new StackFrame(undefined, undefined, match[2], match[1], undefined, lines[i]));
                }
            }

            return result;
        },

        parseOpera10: function ErrorStackParser$$parseOpera10(e) {
            var lineRE = /Line (\d+).*script (?:in )?(\S+)(?:: In function (\S+))?$/i;
            var lines = e.stacktrace.split('\n');
            var result = [];

            for (var i = 0, len = lines.length; i < len; i += 2) {
                var match = lineRE.exec(lines[i]);
                if (match) {
                    result.push(new StackFrame(match[3] || undefined, undefined, match[2], match[1], undefined, lines[i]));
                }
            }

            return result;
        },

        // Opera 10.65+ Error.stack very similar to FF/Safari
        parseOpera11: function ErrorStackParser$$parseOpera11(error) {
            return error.stack.split('\n').filter(function (line) {
                return !!line.match(FIREFOX_SAFARI_STACK_REGEXP) &&
                    !line.match(/^Error created at/);
            }, this).map(function (line) {
                var tokens = line.split('@');
                var locationParts = this.extractLocation(tokens.pop());
                var functionCall = (tokens.shift() || '');
                var functionName = functionCall
                        .replace(/<anonymous function(: (\w+))?>/, '$2')
                        .replace(/\([^\)]*\)/g, '') || undefined;
                var argsRaw;
                if (functionCall.match(/\(([^\)]*)\)/)) {
                    argsRaw = functionCall.replace(/^[^\(]+\(([^\)]*)\)$/, '$1');
                }
                var args = (argsRaw === undefined || argsRaw === '[arguments not available]') ? undefined : argsRaw.split(',');
                return new StackFrame(functionName, args, locationParts[0], locationParts[1], locationParts[2], line);
            }, this);
        }
    };
}));