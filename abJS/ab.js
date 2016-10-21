
/*
 * abJS
 * 
 * Tommaso Bertoni @ 2016
 */

(function () {
	"use strict";
	
	// Functions to create/retrive the list of listeners given a target function's name
    function InitIfABNotSet(targetObj) {
		
		var abMap = targetObj.abMap;
		
		if (!abMap || !(abMap instanceof AddedBehaviours)) {
			// Initialize added-behaviours map
			targetObj.abMap = new AddedBehaviours();
		}
	}
    
	function AddedBehaviours() {
		var absInstance = { _behaviours: [] };
        
        absInstance.addbefore = function(targetFuncName, addedBehaviour) {
            InitIfTargetNotSet(absInstance);
            
            var listeners = absInstance._behaviours[targetFuncName];
            var precedingListeners = listeners.preceding;
            if (!precedingListeners.contains(addedBehaviour))
                precedingListeners.push(addedBehaviour);
        }
        
        absInstance.removebefore = function(targetFuncName, addedBehaviour) {
            InitIfTargetNotSet(absInstance);
            
            var listeners = absInstance._behaviours[targetFuncName];
            var precedingListeners = listeners.preceding;
            if (precedingListeners.contains(addedBehaviour))
                precedingListeners.remove(addedBehaviour);
        }
        
        return absInstance;
	}
    
    function InitIfTargetFuncNotSet(addedBehaviours)
    {
        var listeners = addedBehaviours._behaviours[targetFuncName];
            
        if (!listeners || !(listeners instanceof InvocationListeners)) {
            listeners = new InvocationListeners();
            addedBehaviours._behaviours[targetFuncName] = listeners;
        }
    }
	
	function InvocationListeners() {
	    return {
	        preceding: [],
	    };
	}
	
	// Add add/remove behaviour functions
	Object.prototype.addBefore = function (targetFuncName, addedBehaviour) {
		var self = this;
		InitIfABNotSet(self)
		
		abMap.addBefore(targetFuncName, addedBehaviour);
	}
	
	Object.prototype.removeBefore = function (targetFuncName, addedBehaviour) {
		var self = this;
		InitIfABNotSet(self)
		
		self.abMap.removeBefore(targetFuncName, addedBehaviour);
	}
});

/*
 *  Proposes:
 *      - "after/following" behaviours?
 */