
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

function isjQuery(target) {
    // If jQuery is set and it's the target
    return !!window.jQuery && target === jQuery;
}

function findTargetFunc(target, targetFuncName) {
    if (isjQuery(target) && target.fn !== undefined) {
        return target.fn[targetFuncName];
    } else {
        return target[targetFuncName];
    }
}

function setNewTargetFunc(target, targetFuncName, targetFunc) {

    var newTargetFunc = function () {

        var actualTarget = target;
        if (isjQuery(target)) {
            var elem = this[0]; // Gets the current element
            if (elem)
                actualTarget = elem;
        }

	    var args = arguments;
	    var precedingFuncs = target.abMap.behaviours[targetFuncName].preceding;
	    if (precedingFuncs && precedingFuncs.length > 0) {

	        var result;
	        var async = Q.defer();

	        var promises = [];
	        precedingFuncs.forEach(function (func) { promises.push(Q(func())); });

	        Q.allSettled(promises).then(function () {
	            result = targetFunc.apply($(actualTarget), args);
	            return Q(result);
	        }).then(function () {
	            async.resolve(result);
	        });

	        return async.promise;
	    } else {
	        return asPromise(targetFunc.apply($(actualTarget), args));
	    }
	};

	if (isjQuery(target) && target.fn !== undefined) {
	    target.fn[targetFuncName] = newTargetFunc;
	} else {
	    target[targetFuncName] = newTargetFunc;
	}
}

// Setup global functions
window.ab = function (target) {

    // Add add/remove behaviour functions
    target.addBefore = function (targetFuncName, addedBehaviour) {
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

    target.removeBefore = function (targetFuncName, addedBehaviour) {
        var self = this;
        initIfABNotSet(self, targetFuncName)

        var targetFunc = findTargetFunc(self, targetFuncName);
        if (targetFunc) {
            return self.abMap.detachBefore(targetFuncName, addedBehaviour);
        }

        return false;
    };
}

window.isPromise = function (target) {
    return target.then !== undefined;
};

window.asPromise = function (target) {
    return Q(target);
};

/*
 *  Proposes:
 *      - "after/following" behaviours?
 *      - publish to npm?
 *  
 *  TODOs:
 *      - test with ajax
 *      - test with bootstrap modal popup
 *      - add comments
 */