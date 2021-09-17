﻿const levenshtein = require('js-levenshtein');

export const flattenObject = (origin, destinationDictionnary = {}, prefix = "", excludes=[]) => {
    if (typeof origin === 'string') {
        return null;
    }
    if (Array.isArray(origin)) {
        origin.forEach((e, index) => flattenObject(e, destinationDictionnary, index.toString()+ "_", excludes));
    }
    for (let propertyName in origin) {
        if(excludes.includes(propertyName)){
            continue;
        }
        const value = origin[propertyName];
        if (typeof value === 'string' || value === null) {
            destinationDictionnary[prefix+propertyName] = value;
        } else if (typeof value === 'object') {
            flattenObject(value, destinationDictionnary, prefix+propertyName+"_", excludes);
        }
    }
    return destinationDictionnary;
};

export const levenshteinBetweenTwoDictionnary = (leftDictionnary, rightDictionnary) => {
    const score =  {};
    for (const [key, leftValue] of Object.entries(leftDictionnary)) {
        let rightValue = null;
        if (key in rightDictionnary){
            rightValue = rightDictionnary[key];
        }
        score[key] = levenshtein(leftValue ?? "", rightValue ?? "");
    }
    return score;
}

function parseToObject(body, excludes=[]) {
    try {
        const leftBody = JSON.parse(body);
        return flattenObject(leftBody, {}, "", excludes);
    } catch (e) {
        return {};
    }
}

const updateCompleteness = (resultDict, dic, side) => {
    for(const [key, value] of Object.entries(dic)){
        const isCompletenessOK = (value != null && value !== "");
        const sideObject = side ? resultDict[key].completeness_right : resultDict[key].completeness_left;
        if(sideObject.completeness_ok == null){
            if(isCompletenessOK){
                sideObject.completeness_ok = 1;
                sideObject.completeness_ko = 0;
            }
            else{
                sideObject.completeness_ok = 0;
                sideObject.completeness_ko = 1;
            }
        }
        else{
            if(isCompletenessOK){
                sideObject.completeness_ok += 1;
            }
            else{
                sideObject.completeness_ko += 1;
            }
        }
    }
};

const updateResultDict = (result, dict) => {
    for(const key of Object.keys(dict)){
        if(!(key in result)){
            result[key] = {
                score: null,
                score_ok: null,
                score_ko: null,
                completeness_left: {
                    completeness_ok: null,
                    completeness_ko: null
                },
                completeness_right: {
                    completeness_ok: null,
                    completeness_ko: null
                }
            };
        }
    }
}

export const totalCompletenessByKey = side => {
    let result = Number(0.00).toFixed(2);
    if(side.completeness_ok != null){
        result = (side.completeness_ok * 100 / (side.completeness_ok + side.completeness_ko)).toFixed(2);
    }
    return result;
};

const completenessValue = (side, isOK) => {
    if(isOK){
        return (side.completeness_ok != null ? side.completeness_ok : 0);
    }
    return (side.completeness_ko != null ? side.completeness_ko : 0);
};

const setLevenshteinParameters = levenshteinResult => {
    const result = {};
    Object.keys(levenshteinResult).forEach(function(key){
        const value = levenshteinResult[key];
        const completenessOKLeft = totalCompletenessByKey(levenshteinResult[key].completeness_left);
        const completenessOKRight = totalCompletenessByKey(levenshteinResult[key].completeness_right);
        const score = value.score != null ? value.score : "-";
        const scoreOK = value.score_ok != null ? value.score_ok : 0;
        const scoreKO = value.score_ko != null ? value.score_ko : 0;
        const percentagesOK = ((scoreOK + scoreKO) === 0 ? "-" : `${Math.round((scoreOK / (scoreOK + scoreKO))*100*100)/100} %`);
        result[key] = {
            score,
            score_ok: scoreOK,
            score_ko: scoreKO,
            total: scoreOK + scoreKO,
            completeness_left: {
                ok: completenessValue(value.completeness_left, true),
                ko: completenessValue(value.completeness_left, false)
            },
            completeness_right: {
                ok: completenessValue(value.completeness_right, true),
                ko: completenessValue(value.completeness_right, false)
            },
            percentages: {
                ok: percentagesOK,
                completeness_ok_left: `${completenessOKLeft} %`,
                completeness_ok_right: `${completenessOKRight} %`
            }
        };
    });
    return result;
};

export const totalScores = (items, excludes = []) => {
    const resultDict = {};
    items.forEach(item => {
        const leftDic = parseToObject(item.left.Body, excludes);
        const rightDic = parseToObject(item.right.Body, excludes);
        
        updateResultDict(resultDict, leftDic);
        updateResultDict(resultDict, rightDic);
        updateCompleteness(resultDict, leftDic, false);
        updateCompleteness(resultDict, rightDic, true);
        
        const scoreItem = levenshteinBetweenTwoDictionnary(leftDic, rightDic);
        for (const [key, value] of Object.entries(scoreItem)) {
            resultDict[key].score += scoreItem[key];
            resultDict[key].score_ok += value ===0 ? 1:0;
            resultDict[key].score_ko += value !== 0 ? 1: 0;
        }
    });
    return setLevenshteinParameters(resultDict);
};
