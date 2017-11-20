// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ids.core.membership.plugin.Interfaces
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public interface IMembershipClaimsService
    {
        Task<IEnumerable<Claim>> GetClaimsFromAccount(string username);

        /// <summary>Gets the Claims from the Membership User</summary>
        /// <param name="user">Membership User</param>
        /// <returns>List of Claims</returns>
        Task<IEnumerable<Claim>> GetClaimsFromAccount(MembershipUser user);
    }
}
