
/*
 * abJS
 *
 * MIT License
 * 
 * Copyright (c) 2016 Tommaso Bertoni
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 */
	
// Functions to create/retrive the list of listeners given a target function's name
function initIfABNotSet(target) {
		
    var abMap = target.abMap;
		
	if (!abMap || !(abMap instanceof AddedBehaviours)) {
		// Initialize added-behaviours map
	    target.abMap = new AddedBehaviours();
	}
}

function initIfTargetNotSet(addedBehaviours, targetFuncName) {
    var listeners = addedBehaviours.behaviours[targetFuncName];

    if (!listeners || !(listeners instanceof InvocationListeners)) {
        listeners = new InvocationListeners();
        addedBehaviours.behaviours[targetFuncName] = listeners;
    }
}
    
function AddedBehaviours() {
    var abInstance = this;

    abInstance.behaviours = [];
        
	abInstance.attachBefore = function (targetFuncName, addedBehaviour) {
	    initIfTargetNotSet(abInstance, targetFuncName);
            
		var listeners = abInstance.behaviours[targetFuncName];
        var precedingListeners = listeners.preceding;
        if (!precedingListeners.includes(addedBehaviour)) {
            precedingListeners.push(addedBehaviour);
            return true;
        }

        return false;
    }
        
	abInstance.detachBefore = function (targetFuncName, addedBehaviour) {
	    initIfTargetNotSet(abInstance, targetFuncName);
            
		var listeners = abInstance.behaviours[targetFuncName];
        var precedingListeners = listeners.preceding;
        if (precedingListeners.includes(addedBehaviour)) {
            var index = precedingListeners.indexOf(addedBehaviour);
            if (index > -1) {
                precedingListeners.splice(index, 1); // behaviour removed
                return true;
            }
        }

        return false;
	}

	abInstance.contains = function (targetFuncName) {
	    return abInstance.behaviours[targetFuncName] !== undefined;
	}
        
	return abInstance;
}
	
function InvocationListeners() {
    var instance = this;
    instance.preceding = [];
    return instance;
}

function findTargetFunc(target, targetFuncName) {
    // If jQuery is set and it's the target
    if (!!window.jQuery && target instanceof jQuery) {
        return target.fn[targetFuncName];
    } else {
        return target[targetFuncName];
    }
}

function setNewTargetFunc(target, targetFuncName, targetFunc) {

	var newTargetFunc = function () {

	    var precedingFuncs = target.abMap.behaviours[targetFuncName].preceding;
	    if (precedingFuncs) {

	        var result;
	        var async = Q.defer();

	        var promises = [];
	        precedingFuncs.forEach(function (func) { promises.push(Q(func())); });

	        Q.allSettled(promises).then(function () {
	            result = Q(targetFunc(arguments));
	            return result;
	        }).then(function () {
	            async.resolve(result);
	        });

	        return async.promise;
	    } else {
	        return targetFunc();
	    }
	};

    // If jQuery is set and it's the target
	if (!!window.jQuery && target instanceof jQuery) {
	    target.fn[targetFuncName] = newTargetFunc;
	} else {
	    target[targetFuncName] = newTargetFunc;
	}
}

// Add add/remove behaviour functions
Object.prototype.addBefore = function (targetFuncName, addedBehaviour) {
    var self = this;
    initIfABNotSet(self, targetFuncName)

    var targetFunc = findTargetFunc(self, targetFuncName);
    if (targetFunc) {
        if (!self.abMap.contains(targetFuncName)) {
            setNewTargetFunc(self, targetFuncName, targetFunc);
        }

        return self.abMap.attachBefore(targetFuncName, addedBehaviour);
    }

    return false;
}

Object.prototype.removeBefore = function (targetFuncName, addedBehaviour) {
    var self = this;
    initIfABNotSet(self, targetFuncName)

    var targetFunc = findTargetFunc(self, targetFuncName);
    if (targetFunc) {
        return self.abMap.detachBefore(targetFuncName, addedBehaviour);
    }

    return false;
}

Object.prototype.isPromise = function () {
    var self = this;
    return self.then !== undefined;
}

Object.prototype.asPromise = function () {
    var self = this;
    return Q(self);
}

/*
 *  Proposes:
 *      - "after/following" behaviours?
 */