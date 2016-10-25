
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
    
// Utils
function initIfABNotSet(target) {

    var abMap = target.abMap;
    if (!abMap || !(abMap instanceof AttachedBehaviours))
        target.abMap = new AttachedBehaviours();
}

function initIfTargetNotSet(attachedBehaviours, targetFuncName) {

    var listeners = attachedBehaviours.behaviours[targetFuncName];
    if (!listeners || !(listeners instanceof InvocationListeners)) {
        listeners = new InvocationListeners();
        attachedBehaviours.behaviours[targetFuncName] = listeners;
    }
}

function isjQuery(target) {
    // If jQuery is set and it's the target
    return !!window.jQuery && target === jQuery;
}

function findTargetFunc(target, targetFuncName) {
    if (isjQuery(target) && target.fn !== undefined && target.fn[targetFuncName] !== undefined) {
        // If is jQuery and the function is defined in the fn property
        return target.fn[targetFuncName];
    } else {
        return target[targetFuncName];
    }
}

function setNewTargetFunc(target, targetFuncName, targetFunc) {

    // Define the function that will override the target function
    var newTargetFunc = function () {

        var actualTarget = target;
        if (isjQuery(target)) {
            var elem = this[0]; // Gets the current element
            if (elem)
                actualTarget = $(elem);
        }

        var args = arguments; // Arguments for the target function
        var result;
        var async = Q.defer(); // Promise result

        var promises = [];

        // Execute the "preceding" functions
        var precedingFuncs = target.abMap.behaviours[targetFuncName].preceding;
        if (precedingFuncs) {
            precedingFuncs.forEach(function (func) { promises.push(asPromise(func())); });
        }
        
        Q.allSettled(promises).then(function () {

            // Execute the target function
            result = targetFunc.apply(actualTarget, args);
            return asPromise(result);

        }).then(function () {

            // Resolve the promise result
            async.resolve(result);

            promises = [];

            // Execute the "following" functions
            var followingFuncs = target.abMap.behaviours[targetFuncName].following;
            if (followingFuncs) {
                followingFuncs.forEach(function (func) { promises.push(asPromise(func())); });
            }

            return Q.allSettled(promises);
        });

        return async.promise;
    };

    // Replace the target function

    if (isjQuery(target) && target.fn !== undefined && target.fn[targetFuncName] !== undefined) {
        // If is jQuery and the function is defined in the fn property
        target.fn[targetFuncName] = newTargetFunc;
    } else {
        target[targetFuncName] = newTargetFunc;
    }
}

function setupAddBehaviour(target, targetFuncName, delegateAddBehaviour) {

    initIfABNotSet(target, targetFuncName)
    var targetFunc = findTargetFunc(target, targetFuncName);
    if (targetFunc) {
        if (!target.abMap.contains(targetFuncName))
            setNewTargetFunc(target, targetFuncName, targetFunc);

        return delegateAddBehaviour();
    }

    return false;
}

function setupRemoveBehaviour(target, targetFuncName, delegateRemoveBehaviour) {

    initIfABNotSet(target, targetFuncName)
    var targetFunc = findTargetFunc(target, targetFuncName);
    if (targetFunc)
        return delegateRemoveBehaviour();

    return false;
}
// /Utils

// Types
function AttachedBehaviours() {
    var abInstance = this;

    abInstance.behaviours = []; // FuncName: InvocationListeners map

    abInstance.attachBefore = function (targetFuncName, attachedBehaviour) {

        initIfTargetNotSet(abInstance, targetFuncName);
        var listeners = abInstance.behaviours[targetFuncName];
        var precedingListeners = listeners.preceding;
        if (precedingListeners.indexOf(attachedBehaviour) === -1) { // Add the behaviour only once
            precedingListeners.push(attachedBehaviour);
            return true;
        }

        return false;
    }

    abInstance.detachBefore = function (targetFuncName, attachedBehaviour) {

        initIfTargetNotSet(abInstance, targetFuncName);
        var listeners = abInstance.behaviours[targetFuncName];
        var precedingListeners = listeners.preceding;
        if (precedingListeners.indexOf(attachedBehaviour) > -1) {
            var index = precedingListeners.indexOf(attachedBehaviour);
            if (index > -1) {
                precedingListeners.splice(index, 1);
                return true;
            }
        }

        return false;
    }

    abInstance.attachAfter = function (targetFuncName, attachedBehaviour) {

        initIfTargetNotSet(abInstance, targetFuncName);
        var listeners = abInstance.behaviours[targetFuncName];
        var followingListeners = listeners.following;
        if (followingListeners.indexOf(attachedBehaviour) === -1) { // Add the behaviour only once
            followingListeners.push(attachedBehaviour);
            return true;
        }

        return false;
    }

    abInstance.detachAfter = function (targetFuncName, attachedBehaviour) {

        initIfTargetNotSet(abInstance, targetFuncName);
        var listeners = abInstance.behaviours[targetFuncName];
        var followingListeners = listeners.following;
        if (followingListeners.indexOf(attachedBehaviour) > -1) {
            var index = followingListeners.indexOf(attachedBehaviour);
            if (index > -1) {
                followingListeners.splice(index, 1);
                return true;
            }
        }

        return false;
    }

    // Checks if the target function was already redefined on this target
    abInstance.contains = function (targetFuncName) {
        return abInstance.behaviours[targetFuncName] !== undefined;
    }

    return abInstance;
}

function InvocationListeners() {
    var instance = this;

    // Lists of attached behaviours
    instance.preceding = [];
    instance.following = [];

    return instance;
}
// /Types

// Global functions

// Initialize the target (add functionalities)
window.ab = function (target) {

    target.addBefore = function (targetFuncName, attachedBehaviour) {
        var self = this;
        return setupAddBehaviour(self, targetFuncName, function () {
            return self.abMap.attachBefore(targetFuncName, attachedBehaviour);
        });
    }

    target.removeBefore = function (targetFuncName, attachedBehaviour) {
        var self = this;
        return setupRemoveBehaviour(self, targetFuncName, function () {
            return self.abMap.detachBefore(targetFuncName, attachedBehaviour);
        });
    };

    target.addAfter = function (targetFuncName, attachedBehaviour) {
        var self = this;
        return setupAddBehaviour(self, targetFuncName, function () {
            return self.abMap.attachAfter(targetFuncName, attachedBehaviour);
        });
    }

    target.removeAfter = function (targetFuncName, attachedBehaviour) {
        var self = this;
        return setupRemoveBehaviour(self, targetFuncName, function () {
            return self.abMap.detachAfter(targetFuncName, attachedBehaviour);
        });
    };
}

window.isPromise = function (target) {
    return target.then !== undefined;
};

window.asPromise = function (target) {
    return Q(target);
};
// /Global functions