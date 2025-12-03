function isEmpty(obj) {
  if (typeof obj !== "object" || obj === null) {
    return false;
  }
  return Object.keys(obj).length === 0;
}

const renderListOption = (data, displayCol, valCol) => {
  if (data && data.length > 0) {
    var listOption = data.map((item) => {
      return { label: item[displayCol], value: item[valCol] };
    });

    return listOption;
  }
  return [];
};

const deepEqualObject = (obj1, obj2) => {
  if (obj1 === obj2) return true;

  if (
    obj1 === null ||
    obj2 === null ||
    typeof obj1 !== "object" ||
    typeof obj2 !== "object"
  ) {
    return false;
  }

  const keys1 = Object.keys(obj1);
  const keys2 = Object.keys(obj2);

  if (keys1.length !== keys2.length) return false;

  for (let key of keys1) {
    if (!keys2.includes(key) || !deepEqualObject(obj1[key], obj2[key])) {
      return false;
    }
  }

  return true;
};

function getNumbersFromString(str) {
  return str.replace(/\D/g, "");
}

const fixFormatPhoneNumber = (phoneNumber) => {
  try {
    if (!phoneNumber || phoneNumber.length > 12 || phoneNumber.length < 9) {
      return "";
    }

    phoneNumber = getNumbersFromString(phoneNumber);

    if (phoneNumber[0] === "0") {
      phoneNumber = "84" + phoneNumber.substring(1);
    }

    if (
      phoneNumber[0] !== "0" &&
      (phoneNumber.length === 9 || phoneNumber.length === 10)
    ) {
      phoneNumber = "84" + phoneNumber;
    }

    let result = phoneNumber;
    let dauso = phoneNumber.substring(0, 5);
    let cuoiso = phoneNumber.substring(5);

    const prefixMap = {
      // mobi
      84120: "8470",
      84121: "8479",
      84122: "8477",
      84126: "8476",
      84128: "8478",
      // vina
      84123: "8483",
      84124: "8484",
      84125: "8485",
      84127: "8481",
      84129: "8482",
      // viettel
      84162: "8432",
      84163: "8433",
      84164: "8434",
      84165: "8435",
      84166: "8436",
      84167: "8437",
      84168: "8438",
      84169: "8439",
      // VNM
      84186: "8456",
      84188: "8458",
      // GTEL
      84199: "8459",
    };

    if (prefixMap[dauso]) {
      result = prefixMap[dauso] + cuoiso;
    }

    return result;
  } catch (err) {
    return "";
  }
};

function getIdTelco(sdt) {
  try {
    if (!sdt || sdt.length < 8) return 0;

    sdt = fixFormatPhoneNumber(sdt);
    let len = sdt.length;
    let phonePrefix = sdt.substring(0, 4);
    let phonePrefixKorean = sdt.substring(0, 3);
    let result = 0;

    if (len === 11) {
      const telcoMap11 = {
        // Viettel
        8489: 1,
        8490: 1,
        8493: 1,
        8470: 1,
        8479: 1,
        8477: 1,
        8476: 1,
        8478: 1,
        // Vina
        8491: 2,
        8494: 2,
        8488: 2,
        8483: 2,
        8484: 2,
        8485: 2,
        8481: 2,
        8482: 2,
        // Mobi
        8497: 3,
        8498: 3,
        8496: 3,
        8486: 3,
        8432: 3,
        8433: 3,
        8434: 3,
        8435: 3,
        8436: 3,
        8437: 3,
        8438: 3,
        8439: 3,
        // VNM
        8492: 12,
        8452: 12,
        8456: 12,
        8458: 12,
        // GTEL
        8499: 11,
        8459: 11,
        // Korean
        "010": 6,
        "011": 6,
        "013": 6,
        // Other
        8487: 14,
        8455: 16,
      };

      if (telcoMap11[phonePrefix]) result = telcoMap11[phonePrefix];
      else if (telcoMap11[phonePrefixKorean])
        result = telcoMap11[phonePrefixKorean];
    } else if (len === 12) {
      phonePrefix = sdt.substring(0, 5);
      const telcoMap12 = {
        84126: 1,
        84128: 1,
        84120: 1,
        84121: 1,
        84122: 1,
        84123: 2,
        84124: 2,
        84125: 2,
        84127: 2,
        84129: 2,
        84168: 3,
        84169: 3,
        84166: 3,
        84165: 3,
        84167: 3,
        84164: 3,
        84163: 3,
        84162: 3,
        84186: 12,
        84188: 12,
        84199: 11,
      };

      if (telcoMap12[phonePrefix]) result = telcoMap12[phonePrefix];
    }

    return result;
  } catch (err) {
    return 0;
  }
}

const checkFeatureShow = (listData, name, isExist = true) => {
  try {
    if (listData?.length > 0) {
      var data = listData.find((x) => x.key == name);
      if (data) {
        return data.value;
      }
    }
  } catch (error) {}
  if (isExist) {
    return false;
  } else {
    return true;
  }
};

function isValidEmail(email) {
  const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return regex.test(email);
}

const shortNum = (num) => num > 99 ? "+99" : num.toString();

export {
  isEmpty,
  renderListOption,
  deepEqualObject,
  fixFormatPhoneNumber,
  getIdTelco,
  checkFeatureShow,
  isValidEmail,
  shortNum,
};
